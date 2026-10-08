Samtykke og privatliv
•	2. Som Deltager, ønsker jeg aktivt at skulle give samtykke, før min position deles, så jeg selv bestemmer over mine data.
o	Beskrivelse: Mobil-appen viser en samtykke-knap. Geolocation API kaldes og positioner sendes først, når deltageren har trykket på knappen. Uden samtykke sendes ingen data.
•	3. Som Deltager, ønsker jeg at få at vide, hvilke data der indsamles og hvor længe de gemmes, før jeg giver samtykke, så mit samtykke er informeret.
o	Beskrivelse: Samtykkesiden forklarer kort, at kun position og et anonymt session-ID sendes, hvad data bruges til, og hvor længe de gemmes. Teksten står over samtykke-knappen.
•	7. Som Deltager, ønsker jeg at kunne stoppe delingen, så jeg kan trække mit samtykke til delingen af mine data tilbage.
o	Beskrivelse: En stop-knap afbryder positionsdelingen med det samme og sletter session-ID'et i browseren. Der sendes ikke flere positioner, før deltageren giver samtykke igen.
•	10. Som Deltager, ønsker jeg at delinger stopper automatisk, når eventet slutter, så min position ikke deles unødvendigt.
o	Beskrivelse: Appen kender eventets sluttid og stopper selv delingen, når den er nået, også selvom deltageren ikke har trykket stop.
•	11. Som Deltager, ønsker jeg at kunne se, om min position bliver delt lige nu, så jeg ved, hvornår jeg bidrager.
o	Beskrivelse: Appen viser en tydelig status ("Deler position" / "Deler ikke"), der opdateres, når delingen starter, stoppes eller fejler.
Positionsdeling
•	4. Som Deltager, ønsker jeg at dele min position anonymt, så jeg bidrager til arrangørens visuelle overblik over deres event, uden at jeg kan identificeres.
o	Beskrivelse: Appen sender kun koordinater, tidspunkt og et tilfældigt session-ID. Der sendes og gemmes intet, der kan knyttes til en person, fx navn, IP-adresse eller device-ID.
•	8. Som Arrangør, ønsker jeg at kunne se positioner fra givne zoner, så det visuelle overblik, ikke bliver forurenet af irrelevante data.
o	Beskrivelse: Ingestion-serveren afviser positioner, der ligger uden for et aktivt events område eller tidsrum, så kun relevante positioner når frem til overblikket.
•	16. Som Arrangør, ønsker jeg at én enhed ikke kan sende flere positioner end tilladt, så det visuelle overblik ikke kan manipuleres.
o	Beskrivelse: Ingestion-serveren begrænser, hvor mange positioner et session-ID må sende pr. tidsenhed. Positioner ud over grænsen afvises.
•	17. Som Arrangør, ønsker jeg at kunne bestemme, hvor ofte der bliver sendt/modtaget positionsdata fra deltagere, så det visuelle overblik ikke bliver forurenet af unødvendige data.
o	Beskrivelse: Arrangøren angiver sendeintervallet for et event i dashboardet. Mobil-appen henter intervallet og sender positioner i det tempo.
Eventadministration
•	1. Som Arrangør, ønsker jeg at kunne oprette et event med navn, start- og sluttid og et område tegnet på et kort, så systemet ved, hvor og hvornår der skal indsamles data.
o	Beskrivelse: I dashboardet udfylder arrangøren navn, start- og sluttid og tegner eventets område som en polygon på et kort. Eventet gemmes og bliver kendt af ingestion-serveren.
•	9. Som Arrangør, ønsker jeg, at et event, jeg opretter eller redigerer, inden for 30 sekunder modtager positionsdata (hvis inden for givent tidsinterval), så jeg ikke skal vente på noget eller genstarte noget.
o	Beskrivelse: Når et event oprettes eller ændres, sendes ændringen automatisk til ingestion-serveren, som herefter accepterer positioner til eventet uden genstart.
•	12. Som Arrangør, ønsker jeg at se en liste over kommende og aktive events, så jeg kan vælge, hvilket event jeg vil overvåge.
o	Beskrivelse: Dashboardet viser en liste over events, der ikke er afsluttet, med navn, tidsrum og status (kommende/aktiv). Et klik åbner eventets overblik.
•	21. Som Arrangør, ønsker jeg at kunne redigere et events navn, start- og sluttid, samt lokationen, så jeg slipper for at slette og oprette på ny som alternativ.
o	Beskrivelse: Arrangøren kan åbne et eksisterende event og ændre navn, tidsrum og område. Ændringerne gemmes og slår igennem i ingestion-serveren.
•	23. Som Arrangør, ønsker jeg at kunne slette et event, så jeg kan fjerne aflyste eller fejloprettede events.
o	Beskrivelse: Arrangøren kan slette et event efter at have bekræftet det. Eventet forsvinder fra listen, og ingestion-serveren holder op med at modtage positioner til det.
Zoneadministration
•	13. Som Arrangør, ønsker jeg at kunne oprette zoner til mit event, så jeg kan holde styr på, hvor deltagere befinder sig i enkelte zoner.
o	Beskrivelse: Arrangøren tegner en eller flere navngivne zoner (fx "Scene A", "Indgang") inden for eventets område på kortet.
•	14. Som Arrangør, ønsker jeg at en zone jeg opretter, straks vises med antal deltagere i det visuelle overblik, så jeg ikke skal vente på noget eller genstarte noget.
o	Beskrivelse: En ny zone dukker op i det live overblik med det aktuelle antal deltagere, uden at siden eller serveren skal genstartes.
•	15. Som Arrangør, ønsker jeg at kunne sætte en maks. begrænsning på antallet af deltagere i en zone og blive underrettet, hvis antallet af deltagere overskrider denne begrænsning, så jeg kan opdage farlige ophobninger.
o	Beskrivelse: Hver zone kan have en kapacitet. Når antallet af deltagere i zonen overstiger kapaciteten, markeres zonen tydeligt, og arrangøren får en notifikation i dashboardet.
•	22. Som Arrangør, ønsker jeg at kunne redigere zoner i mit event, så jeg kan rette en forkert tegnet zone eller ændre dens kapacitet.
o	Beskrivelse: Arrangøren kan ændre en zones navn, område og kapacitet. Overblikket bruger de nye værdier med det samme.
•	24. Som Arrangør, ønsker jeg at kunne slette zoner fra mit event, så jeg kan fjerne zoner, der ikke længere er relevante.
o	Beskrivelse: Arrangøren kan slette en zone efter at have bekræftet det. Zonen forsvinder fra overblikket.
Live overblik
•	5. Som Arrangør, ønsker jeg at kunne se de nuværende positionsdata i et visuelt overblik, så jeg for det enkelte event samt zoneopdelinger kan se, hvordan deltagermængder bevæger sig rundt.
o	Beskrivelse: Dashboardet viser et heatmap over eventets område, hvor positioner er samlet i grid-celler, og antal deltagere pr. zone. Det opdateres løbende uden at genindlæse siden.
Historik og datahåndtering
•	25. Som Arrangør, ønsker jeg at kunne tilgå en historik over positionsdata i givne tidsintervaller, så jeg for det enkelte event samt dets zoner kan se, hvordan deltagermængder har bevæget sig rundt.
o	Beskrivelse: Arrangøren vælger et event og et tidsinterval og kan afspille eller bladre i heatmappet og zonetallene for den periode.
•	26. Som Arrangør, ønsker jeg at kunne tilgå en historik over allerede afholdte events, så jeg kan se tidligere events' informationer vedr. navn, start- og sluttid og et område tegnet på et kort.
o	Beskrivelse: Dashboardet har en liste over afsluttede events. Arrangøren kan åbne et event og se navn, tidsrum og område på kortet.
•	27. Som Arrangør, ønsker jeg at data automatisk bliver slettet efter 24 måneder, så jeg ikke behøver at lagre unødvendige data.
o	Beskrivelse: Systemet sletter automatisk aggregerede heatmapdata, der er ældre end 24 måneder, uden at arrangøren skal gøre noget.
Konto og adgang
•	6. Som Arrangør, ønsker jeg at kunne logge ind på en konto, så kun autoriserede personer har adgang til det visuelle overblik. (Husk at tilføj til acceptkriterie at det er en forudsætning at systemet leves med én præoprettet arrangørkonto)
o	Beskrivelse: Dashboardet kræver login med brugernavn og kodeord. Systemet leveres med én præoprettet arrangørkonto. Uden login er der ingen adgang til dashboard eller API.
•	18. Som Arrangør, ønsker jeg at kunne ændre kodeordet til min konto, så jeg kan beskytte min konto med mit eget personlige kodeord.
o	Beskrivelse: Når arrangøren er logget ind, kan vedkommende skifte kodeord ved at angive det nuværende og et nyt kodeord.
•	19. Som Arrangør, ønsker jeg at blive bedt om at ændre det udleverede kodeord ved første login, så kun jeg kender kodeordet til min konto.
o	Beskrivelse: Ved første login sendes arrangøren direkte til en side, hvor kodeordet skal skiftes, før resten af dashboardet kan bruges.
•	20. Som Arrangør, ønsker jeg at kunne logge ud af min konto, så uvedkommende ikke kan tilgå overblikket fra den computer, jeg har brugt.
o	Beskrivelse: En log ud-knap afslutter sessionen. Herefter kræver dashboardet login igen.