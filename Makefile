DAY=$$(date +"%d")
YEAR=$$(date +"%Y")
DAY_SHORT=$$(echo $(DAY) | sed 's/^0*//')
TOKEN=$$(cat cookie)

.PHONY: csharp
csharp:
	echo $(DAY)
	cp -R charp $(DAY)
	mv $(DAY)/tmp.csproj $(DAY)/aoc$(DAY).csproj
	rm -rf $(DAY)/bin
	rm -rf $(DAY)/obj
	mv $(DAY)/Program.cs $(DAY)/Day$(DAY).cs
	curl --cookie session=$(TOKEN) https://adventofcode.com/$(YEAR)/day/$(DAY_SHORT)/input > $(DAY)/input.txt


.PHONY: kotlin
kotlin:
	echo $(DAY)
	cp -R kotlin $(DAY)
	mv $(DAY)/tmp.iml $(DAY)/aoc$(DAY).iml
	sed -i "s/tmp/aoc$(DAY)/g" ./$(DAY)/.idea/modules.xml
	curl --cookie session=$(TOKEN) https://adventofcode.com/$(YEAR)/day/$(DAY_SHORT)/input > $(DAY)/input.txt

.PHONY: token
token:
	echo $(TOKEN)
