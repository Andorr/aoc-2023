using utils;
var input = parse("input.txt");
Console.WriteLine($"Part01: {part01(input)}");
Console.WriteLine($"Part02: {part02(input)}");


long part01(List<Hand> input)
{
    input.Sort((a, b) => a.cards.CompareTo(b.cards));

    return input.Select((hand, rank) => hand.bid * ((long)rank + 1L)).Sum();
}

long part02(List<Hand> input)
{
    // Change the value of ennum Card.J to 1
    var l = input.Select(hand => (new JokerCards(hand.cards.cards), hand.bid)).ToList();
    l.Sort((a, b) => a.Item1.CompareTo(b.Item1));
    return l.Select((hand, rank) => hand.bid * ((long)rank + 1L)).Sum();
}

List<Hand> parse(string fileName)
{
    return File.ReadAllLines(fileName)
    .Where(line => !string.IsNullOrWhiteSpace(line))
    .Select(line =>
    {
        var cards = line
            .Take(5)
            .Select(c => (Card)Enum.Parse(typeof(Card), c.ToString()))
            .ToLst();
        var bid = line.Split(" ").Last().Let(n => long.Parse(n));
        return new Hand(new NormalCards(cards), bid);
    })
    .ToList();
}


enum Card
{
    None = 0,
    A = 14,
    K = 13,
    Q = 12,
    J = 11,
    T = 10,
    _9 = 9,
    _8 = 8,
    _7 = 7,
    _6 = 6,
    _5 = 5,
    _4 = 4,
    _3 = 3,
    _2 = 2
}

record JokerCards(Lst<Card> cards) : Cards, IComparable<Cards>
{
    private IEnumerable<IGrouping<Card, Card>> groups => cards.GroupBy(c => c);
    // Group without Joker
    private IEnumerable<IGrouping<Card, Card>> groupsWithoutJoker => cards.Where(c => c != Card.J).GroupBy(c => c);

    // Group of J
    private int jokerCnt => cards.Where(c => c == Card.J).Count();

    private bool IsFlush => cards.All(c => c == cards[0]) || groupsWithoutJoker.Any(c => c.Count() + jokerCnt == 5);
    private bool FourOfAKind => groups.Any(g => g.Count() == 4) || groupsWithoutJoker.Any(g => g.Count() + jokerCnt == 4);
    private bool FullHouse()
    {
        var isFullHouse = groups
        .Any(g => g.Count() == 3) &&
            groups.Any(g => g.Count() == 2);
        if (isFullHouse)
        {
            return isFullHouse;
        }

        var c = groupsWithoutJoker.Where(g => g.Count() == 3).Select(g => g.Key).FirstOrDefault();
        if (c != Card.None)
        {
            return groupsWithoutJoker.Where(g => g.Key != c).Any(g => g.Count() + jokerCnt == 2);
        }

        c = groupsWithoutJoker.Where(g => g.Count() == 2).Select(g => g.Key).FirstOrDefault();
        if (c != Card.None)
        {
            return groupsWithoutJoker.Where(g => g.Key != c).Any(g => g.Count() + jokerCnt == 3);
        }

        return false;
    }

    private bool ThreeOfAKind => groups.Any(g => g.Count() == 3) || groupsWithoutJoker.Any(g => g.Count() + jokerCnt == 3);
    private bool TwoPairs => groups.Where(g => g.Count() == 2).Let(g => g.Count() == 2 || g.Count() + jokerCnt >= 2);
    private bool OnePair => groups.Any(g => g.Count() == 2) || groupsWithoutJoker.Any(g => g.Count() + jokerCnt >= 2);
    private bool HighCard => groups.All(g => g.Count() == 1) || groupsWithoutJoker.All(g => g.Count() == 1);

    public int CardRank()
    {
        if (IsFlush) return 7;
        else if (FourOfAKind) return 6;
        else if (FullHouse()) return 5;
        else if (ThreeOfAKind) return 4;
        else if (TwoPairs) return 3;
        else if (OnePair) return 2;
        else if (HighCard) return 1;
        return 0;
    }

    public int CompareTo(Cards other)
    {
        var cardRank = CardRank();
        var otherCardRank = other.CardRank();
        if (cardRank == otherCardRank)
        {
            var result = cards.Select((c, i) => (c != other.cards[i], i))
                .Where(c => c.Item1);
            if (result.Count() == 0)
            {
                return 0;
            }
            var first = result.First();

            var value = cards[first.i] == Card.J ? 1 : (int)cards[first.i];
            var otherValue = other.cards[first.i] == Card.J ? 1 : (int)other.cards[first.i];

            return value.CompareTo(otherValue);
        }
        return cardRank.CompareTo(otherCardRank);
    }
}

record NormalCards(Lst<Card> cards) : Cards, IComparable<Cards>
{
    private IEnumerable<IGrouping<Card, Card>> groups => cards.GroupBy(c => c);

    private bool IsFlush => cards.All(c => c == cards[0]);
    private bool FourOfAKind => groups.Any(g => g.Count() == 4);
    private bool FullHouse => cards
        .GroupBy(c => c)
        .Any(g => g.Count() == 3) &&
            groups.Any(g => g.Count() == 2);
    private bool ThreeOfAKind => groups.Any(g => g.Count() == 3);
    private bool TwoPairs => cards
        .GroupBy(c => c)
        .Where(g => g.Count() == 2)
        .Count() == 2;
    private bool OnePair => groups.Any(g => g.Count() == 2);
    private bool HighCard => groups.All(g => g.Count() == 1);

    public int CardRank()
    {
        if (IsFlush) return 7;
        else if (FourOfAKind) return 6;
        else if (FullHouse) return 5;
        else if (ThreeOfAKind) return 4;
        else if (TwoPairs) return 3;
        else if (OnePair) return 2;
        else if (HighCard) return 1;
        return 0;
    }

    public int CompareTo(Cards other)
    {
        // Their CardRank is equal, then compare the first card
        var cardRank = CardRank();
        var otherCardRank = other.CardRank();
        if (cardRank == otherCardRank)
        {
            var result = cards.Select((c, i) => (c != other.cards[i], i))
                .Where(c => c.Item1);
            if (result.Count() == 0)
            {
                return 0;
            }
            var first = result.First();

            return cards[first.i].CompareTo(other.cards[first.i]);
        }
        return cardRank.CompareTo(otherCardRank);
    }
}

interface Cards : IComparable<Cards>
{
    public Lst<Card> cards { get; }
    public int CardRank();
    // public int CompareTo(Cards other);
}

record Hand(Cards cards, long bid);

