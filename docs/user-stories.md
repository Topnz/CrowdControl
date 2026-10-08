ID-5 Håndtér samtykke & privatliv
•	ID-14. Som Deltager, ønsker jeg aktivt at skulle give samtykke, før min position deles, så jeg selv bestemmer over mine data.
o	Beskrivelse: Mobil-appen viser en samtykke-knap. Geolocation API kaldes og positioner sendes først, når deltageren har trykket på knappen. Uden samtykke sendes ingen data.
o	Acceptkriterier: Appen viser en samtykke-knap. Geolocation API kaldes ikke, før der er trykket på knappen. Positionsdata sendes først efter samtykke. Uden samtykke sendes ingen data.
•	ID-15. Som Deltager, ønsker jeg at få at vide, hvilke data der indsamles og hvor længe de gemmes, før jeg giver samtykke, så mit samtykke er informeret.
o	Beskrivelse: Samtykkesiden forklarer kort, at kun position og et anonymt session-ID sendes, hvad data bruges til, og hvor længe de gemmes. Teksten står over samtykke-knappen.
o	Acceptkriterier: Siden forklarer, hvilke data der indsamles, og hvad de bruges til. Siden oplyser, at kun position og et anonymt session-ID sendes. Siden oplyser, hvor længe data gemmes. Teksten står over samtykke-knappen.
•	ID-18. Som Deltager, ønsker jeg at kunne stoppe delingen, så jeg kan trække mit samtykke til delingen af mine data tilbage.
o	Beskrivelse: En stop-knap afbryder positionsdelingen med det samme og sletter session-ID'et i browseren. Der sendes ikke flere positioner, før deltageren giver samtykke igen.
o	Acceptkriterier: Stop-knappen stopper delingen med det samme. Session-ID'et slettes i browseren. Der sendes ikke flere positioner, før deltageren giver samtykke igen.
•	ID-21. Som Deltager, ønsker jeg at delinger stopper automatisk, når eventet slutter, så min position ikke deles unødvendigt.
o	Beskrivelse: Appen kender eventets sluttid og stopper selv delingen, når den er nået, også selvom deltageren ikke har trykket stop.
o	Acceptkriterier: Appen kender eventets sluttid. Delingen stopper automatisk, når eventet slutter, også uden at deltageren har trukket sit samtykke tilbage.
•	ID-22. Som Deltager, ønsker jeg at kunne se, om min position bliver delt lige nu, så jeg ved, hvornår jeg bidrager.
o	Beskrivelse: Appen viser en tydelig status ("Deler position" / "Deler ikke"), der opdateres, når delingen starter, stoppes eller fejler.
o	Acceptkriterier: Appen viser status "Deler position" eller "Deler ikke". Status opdateres, når delingen starter, stopper eller fejler.
ID-6 Del position
•	ID-16. Som Deltager, ønsker jeg at dele min position anonymt, så jeg bidrager til arrangørens visuelle overblik over deres event, uden at jeg kan identificeres.
o	Beskrivelse: Appen sender kun koordinater, tidspunkt og et tilfældigt session-ID. Der sendes og gemmes intet, der kan knyttes til en person, fx navn, IP-adresse eller device-ID.
o	Acceptkriterier: Appen sender kun koordinater, tidspunkt og et tilfældigt session-ID. Appen sender eller gemmer ingen oplysninger, der kan knyttes til en person (fx navn, IP-adresse eller device-ID).
•	ID-20. Som Arrangør, ønsker jeg at kunne se antal af deltagere i givne zoner, så det visuelle overblik, ikke bliver forurenet af irrelevante data.
o	Beskrivelse: Overblikket viser kun antal deltagere for de valgte zoner, og kun zoner, der hører til eventet, sendes til overblikket.
o	Acceptkriterier: Der vises kun data for de valgte zoner. Kun zoner, der er relevante for eventet, sendes til overblikket.
•	ID-28. Som Arrangør, ønsker jeg at én enhed ikke kan sende flere positioner end tilladt, så det visuelle overblik ikke kan manipuleres.
o	Beskrivelse: Ingestion-serveren begrænser, hvor mange positioner et session-ID må sende pr. tidsenhed. Positioner ud over grænsen afvises.
o	Acceptkriterier: Ingestion-serveren håndhæver en grænse for, hvor mange positioner et session-ID må sende pr. tidsenhed. Positioner ud over grænsen afvises.
•	ID-29. Som Arrangør, ønsker jeg at kunne bestemme, hvor ofte der bliver sendt/modtaget positionsdata fra deltagere, så det visuelle overblik ikke bliver forurenet af unødvendige data.
o	Beskrivelse: Arrangøren angiver sendeintervallet for et event i dashboardet. Mobil-appen henter intervallet og sender positioner i det tempo.
o	Acceptkriterier: Arrangøren kan angive sendeintervallet i dashboardet. Mobil-appen henter intervallet for eventet. Mobil-appen sender positioner i det angivne interval.
•	ID-40. Som Arrangør ønsker jeg, at positionsdataen indeholder nøjagtighed i meter, for at jeg korrekt kan vide, om de pågældende data falder inden for en zone, med en vis sandsynlighed, så jeg kan frasortere irrelevante data.
o	Beskrivelse: Hver position har en nøjagtighed i meter fra mobilens Geolocation API. Arrangøren kan se nøjagtigheden for data fra den enkelte enhed og frasortere upræcise data, så det kan vurderes, om en position med en vis sandsynlighed ligger inden for en zone.
o	Acceptkriterier: Positionsdata indeholder nøjagtighed i meter. Arrangøren kan se nøjagtigheden for data fra den enkelte enhed. Upræcise data kan frasorteres.
ID-7 Administrér event
•	ID-13. Som Arrangør, ønsker jeg at kunne oprette et event med navn, start- og sluttid og et område tegnet på et kort, så systemet ved, hvor og hvornår der skal indsamles data.
o	Beskrivelse: I dashboardet udfylder arrangøren navn, start- og sluttid og tegner eventets område som en polygon på et kort. Eventet gemmes og bliver kendt af ingestion-serveren.
o	Acceptkriterier: Arrangøren kan angive navn, starttid og sluttid. Arrangøren kan tegne eventets område som en polygon på et kort. Eventet gemmes. Ingestion-serveren kender eventet.
•	ID-23. Som Arrangør, ønsker jeg, at et event, jeg opretter eller redigerer, inden for 30 sekunder modtager positionsdata (hvis inden for givent tidsinterval), så jeg ikke skal vente på noget eller genstarte noget.
o	Beskrivelse: Når et event oprettes eller ændres, sendes ændringen automatisk til ingestion-serveren, som herefter accepterer positioner til eventet uden genstart.
o	Acceptkriterier: Ændringen sendes automatisk til ingestion-serveren. Ingestion-serveren accepterer positioner til eventet uden genstart. Eventet modtager positionsdata inden for 30 sekunder, hvis det er inden for tidsrummet.
•	ID-24. Som Arrangør, ønsker jeg at se en liste over kommende og aktive events, så jeg kan vælge, hvilket event jeg vil overvåge.
o	Beskrivelse: Dashboardet viser en liste over events, der ikke er afsluttet, med navn, tidsrum, tilhørende zoner og status (kommende/aktiv). Et klik åbner eventets overblik.
o	Acceptkriterier: Dashboardet viser events, der ikke er afsluttet. Hvert event viser navn, tidsrum, tilhørende zoner og status (kommende eller aktiv). Et klik på et event åbner eventets overblik.
•	ID-33. Som Arrangør, ønsker jeg at kunne redigere et events navn, start- og sluttid, samt området, så jeg slipper for at slette og oprette på ny som alternativ.
o	Beskrivelse: Arrangøren kan åbne et eksisterende event og ændre navn, tidsrum og område. Ændringerne gemmes og slår igennem i ingestion-serveren.
o	Acceptkriterier: Arrangøren kan åbne et eksisterende event. Arrangøren kan ændre navn, tidsrum og område. Ændringerne gemmes og slår igennem i ingestion-serveren.
•	ID-35. Som Arrangør, ønsker jeg at kunne slette et event, så jeg kan fjerne aflyste eller fejloprettede events.
o	Beskrivelse: Arrangøren kan slette et event efter at have bekræftet det. Eventet forsvinder fra listen, og ingestion-serveren holder op med at modtage positioner til det.
o	Acceptkriterier: Arrangøren kan slette et event efter bekræftelse. Eventet fjernes fra listen. Ingestion-serveren holder op med at modtage positioner til det slettede event.
ID-8 Administrér zone
•	ID-25. Som Arrangør, ønsker jeg at kunne oprette zoner til mit event, så jeg kan holde styr på, hvor deltagere befinder sig i enkelte zoner.
o	Beskrivelse: Arrangøren tegner en eller flere navngivne zoner (fx "Bøgescenen Sektion A", "Kærligheden Sektion D") inden for eventets område på kortet.
o	Acceptkriterier: Arrangøren kan oprette en eller flere zoner til et event. Hver zone kan få et navn. Zonerne tegnes inden for eventets område på kortet.
•	ID-26. Som Arrangør, ønsker jeg at en zone jeg opretter, straks vises med antal deltagere i det visuelle overblik, så jeg ikke skal vente på noget eller genstarte noget.
o	Beskrivelse: En ny zone dukker op i det live overblik med det aktuelle antal deltagere, uden at siden eller serveren skal genstartes.
o	Acceptkriterier: En ny zone vises i det live overblik straks efter oprettelse. Overblikket viser det aktuelle antal deltagere i zonen. Siden skal ikke genindlæses, og serveren skal ikke genstartes.
•	ID-27. Som Arrangør, ønsker jeg at kunne sætte en maks. begrænsning på antallet af deltagere i en zone og blive underrettet, hvis antallet af deltagere rammer 90% af denne begrænsning, så jeg kan opdage farlige og undgå ophobninger.
o	Beskrivelse: Hver zone kan have en kapacitet. Arrangøren får en notifikation i dashboardet, når antallet af deltagere når 90 % af kapaciteten, og zonen markeres tydeligt, når kapaciteten overskrides.
o	Acceptkriterier: En zone kan have en kapacitetsgrænse. Når antallet af deltagere overstiger kapaciteten, markeres zonen tydeligt. Når antallet når 90 % af kapaciteten, får arrangøren en notifikation i dashboardet.
•	ID-34. Som Arrangør, ønsker jeg at kunne redigere zoner i mit event, så jeg kan rette en forkert tegnet zone eller ændre dens kapacitet.
o	Beskrivelse: Arrangøren kan ændre en zones navn, område og kapacitet. Overblikket bruger de nye værdier med det samme.
o	Acceptkriterier: Arrangøren kan ændre zonens navn, område og kapacitet. Overblikket bruger de nye værdier straks efter ændringen.
•	ID-36. Som Arrangør, ønsker jeg at kunne slette zoner fra mit event, så jeg kan fjerne zoner, der ikke længere er relevante.
o	Beskrivelse: Arrangøren kan slette en zone efter at have bekræftet det. Zonen forsvinder fra overblikket.
o	Acceptkriterier: Arrangøren kan slette en zone. Sletningen skal bekræftes. Efter sletning er zonen fjernet fra overblikket.
ID-9 Overvåg event
•	ID-17. Som Arrangør, ønsker jeg at kunne se de nuværende positionsdata i et visuelt overblik, så jeg for det enkelte event samt zoneopdelinger kan se, hvordan deltagermængder bevæger sig rundt.
o	Beskrivelse: Dashboardet viser et heatmap over eventets område, hvor positioner er samlet i grid-celler, og antal deltagere pr. zone. Det opdateres, hver gang der kommer nye data, uden at siden genindlæses.
o	Acceptkriterier: Positionsdata samles i grid-celler. Grid-data bruges til at vise heatmappet over eventets område i dashboardet. Antal deltagere vises pr. zone. Dashboardet opdaterer sig selv, hver gang der kommer nye data, uden at siden genindlæses.
•	ID-41. Som Arrangør, ønsker jeg, at positionsdata, der ligger udenfor event-området og event-tidsrum, bliver slettet, så kun relevant data vil blive vist i det visuelle overblik.
o	Beskrivelse: Positioner uden for eventets område eller tidsrum frasorteres og indgår ikke i det visuelle overblik. Ingestion-serveren afviser dem, så de hverken sendes videre eller gemmes.
o	Acceptkriterier: Positionsdata uden for eventets område slettes. Positionsdata uden for eventets tidsrum slettes. Kun relevante data vises i det visuelle overblik.
ID-10 Se historik
•	ID-37. Som Arrangør, ønsker jeg at kunne tilgå en historik over positionsdata i givne tidsintervaller, så jeg for det enkelte event samt dets zoner kan se, hvordan deltagermængder har bevæget sig rundt.
o	Beskrivelse: Arrangøren vælger et event og et tidsinterval og kan afspille eller bladre i heatmappet og zonetallene for den periode.
o	Acceptkriterier: Arrangøren kan vælge et event og et tidsinterval. Arrangøren kan afspille og bladre i heatmappet for perioden. Arrangøren kan se zonetallene for perioden. Det gælder både eventet som helhed og dets zoner.
•	ID-38. Som Arrangør, ønsker jeg at kunne tilgå en historik over allerede afholdte events, så jeg kan se tidligere events' informationer vedr. navn, start- og sluttid og et område tegnet på et kort.
o	Beskrivelse: Dashboardet har en liste over afsluttede events. Arrangøren kan åbne et event og se navn, tidsrum og område på kortet.
o	Acceptkriterier: Dashboardet viser en liste over afsluttede events. Arrangøren kan åbne et afsluttet event. Visningen viser navn, tidsrum og området tegnet på et kort.
•	ID-39. Som Arrangør, ønsker jeg at data automatisk bliver slettet efter 24 måneder, så jeg ikke behøver at lagre unødvendige data.
o	Beskrivelse: Systemet sletter automatisk aggregerede heatmapdata, der er ældre end 24 måneder, uden at arrangøren skal gøre noget.
o	Acceptkriterier: Aggregerede heatmapdata ældre end 24 måneder slettes automatisk. Arrangøren skal ikke gøre noget.
o	Note (fra Jira): Måske kan et snapshot gemmes til sammenligning over årene?
ID-11 Log ind & administrér konto
•	ID-19. Som Arrangør, ønsker jeg at kunne logge ind på en konto, så kun autoriserede personer har adgang til det visuelle overblik.
o	Beskrivelse: Dashboardet kræver login med brugernavn og kodeord. Systemet leveres med én præoprettet arrangørkonto. Uden login er der ingen adgang til dashboard eller API.
o	Acceptkriterier: Dashboardet kræver login. Systemet leveres med én præoprettet arrangørkonto. Uden login er der ingen adgang til dashboardet eller API'et.
•	ID-30. Som Arrangør, ønsker jeg at kunne ændre kodeordet til min konto, så jeg kan beskytte min konto med mit eget personlige kodeord.
o	Beskrivelse: Når arrangøren er logget ind, kan vedkommende skifte kodeord ved at angive det nuværende og et nyt kodeord.
o	Acceptkriterier: Arrangøren skal være logget ind. Arrangøren kan skifte kodeord. Skiftet kræver det nuværende og et nyt kodeord.
•	ID-31. Som Arrangør, ønsker jeg at blive bedt om at ændre det udleverede kodeord ved første login, så kun jeg kender kodeordet til min konto.
o	Beskrivelse: Ved første login sendes arrangøren direkte til en side, hvor kodeordet skal skiftes, før resten af dashboardet kan bruges.
o	Acceptkriterier: Ved første login sendes arrangøren til en side for kodeordsskift. Kodeordet skal skiftes, før resten af dashboardet kan bruges.
•	ID-32. Som Arrangør, ønsker jeg at kunne logge ud af min konto, så uvedkommende ikke kan tilgå overblikket fra den computer, jeg har brugt.
o	Beskrivelse: En log ud-knap afslutter sessionen. Herefter kræver dashboardet login igen.
o	Acceptkriterier: Der er en log ud-knap. Et klik afslutter den aktuelle session. Efter log ud kræver dashboardet login igen.


Use cases
UC1 – Håndtér samtykke & privatliv (ID-5)
•	Aktør: Deltager
•	Summary: Deltageren åbner mobil-appen og kan læse, hvilke data der indsamles (position og et anonymt session-ID), og at de gemmes i 24 måneder. Positionen deles først, når deltageren aktivt giver samtykke. Appen viser hele tiden, om positionen deles. Deltageren kan stoppe delingen når som helst, og den stopper automatisk, når eventet slutter.
•	Stories: ID-14, ID-15, ID-18, ID-21, ID-22
UC2 – Del position (ID-6)
•	Aktør: Deltager (primær), Arrangør (sekundær)
•	Summary: Når deltageren har givet samtykke, sender appen sin position anonymt i det interval, arrangøren har valgt for eventet. Ingestion-serveren tager kun imod positioner inden for et aktivt events område og tidsrum. Den afviser positioner fra en enhed, der sender oftere end tilladt. De godkendte positioner sendes videre til behandling.
•	Stories: ID-16, ID-20, ID-28, ID-29, ID-40
UC3 – Administrér event (ID-7)
•	Aktør: Arrangør
•	Summary: Arrangøren opretter et event med navn, start- og sluttid, sendeinterval og et område tegnet på et kort. Kommende og aktive events vises i en liste, hvor de kan redigeres eller slettes. Ændringer når ingestion-serveren inden for 30 sekunder, så positioner tages imod eller afvises uden genstart.
•	Stories: ID-13, ID-23, ID-24, ID-33, ID-35
UC4 – Administrér zone (ID-8)
•	Aktør: Arrangør
•	Summary: Arrangøren tegner navngivne zoner inden for et events område og kan give hver zone en maksimal kapacitet. Zoner kan redigeres og slettes. En ny zone vises med det samme i overblikket med antal deltagere. Arrangøren får besked, når en zone er over sin kapacitet.
•	Stories: ID-25, ID-26, ID-27, ID-34, ID-36
UC5 – Overvåg event (ID-9)
•	Aktør: Arrangør
•	Summary: Arrangøren vælger et aktivt event og ser et heatmap over, hvor deltagerne befinder sig, samt antal deltagere pr. zone. Overblikket opdateres løbende uden at genindlæse siden, så arrangøren kan se, hvordan mængderne bevæger sig, og opdage farlige ophobninger.
•	Stories: ID-17, ID-41
UC6 – Se historik (ID-10)
•	Aktør: Arrangør (primær), System (automatisk sletning)
•	Summary: Arrangøren kan åbne afsluttede events og se navn, tidsrum og område. For et valgt event og tidsinterval kan arrangøren afspille heatmappet og zonetallene for at evaluere flowet. Systemet sletter automatisk aggregerede data, der er ældre end 24 måneder.
•	Stories: ID-37, ID-38, ID-39
UC7 – Log ind & administrér konto (ID-11)
•	Aktør: Arrangør
•	Summary: Systemet leveres med én præoprettet arrangørkonto. Arrangøren logger ind for at få adgang til dashboardet og skal skifte det udleverede kodeord ved første login. Bagefter kan arrangøren selv ændre kodeordet og logge ud, så uvedkommende ikke får adgang fra den samme computer.
•	Stories: ID-19, ID-30, ID-31, ID-32
