# Simulator infrastrukturnog sistema — PZ2

**Student:** Dušan Kovačević, PR 6/2023
**Kombinacija:** `CG3 · T7 · G3 · P1`

| Oznaka | Značenje | Realizacija u projektu |
|--------|----------|------------------------|
| **CG3** | Korisnici mobilnih telefona | Prozor u *portrait* obliku (emulator telefona), stalno dostupno **Home** dugme, **Undo** dugme koje poništava jednu po jednu akciju kroz celu istoriju (uključujući povratak na prethodni prikaz), i **virtuelna tastatura** (sopstvena, bez NuGet paketa) za unos teksta. |
| **T7** | Temperatura reaktora | Entiteti su oprema za merenje temperature; tipovi **RTD** i **TermoSprega**; validan opseg **250–350 °C**. |
| **G3** | Graf krugova različitih poluprečnika | Krugovi poređani po vremenskoj osi, centri poravnati po X-osi, poluprečnik srazmeran vrednosti; programski iscrtan (bez gotovih Chart kontrola). |
| **P1** | Pretraga | Dva RadioButton-a („Naziv" / „Tip") + TextBox; prikazuju se entiteti čiji se naziv ili naziv tipa delom/kompletno poklapaju s unetim tekstom. |

## Pokretanje

Potrebno je **Visual Studio 2022** sa **.NET 8 SDK** (workload „.NET Desktop Development").

1. Otvoriti `SimulatorInfrastrukturnogSistema.sln`.
2. Build cele solucije (`Ctrl+Shift+B`) — grade se oba projekta: `NetworkService` (WPF aplikacija) i `MeteringSimulator` (konzolni „simulator").
3. Postaviti `NetworkService` kao startup projekat i pokrenuti (`F5`).

`NetworkService` pri startu podiže TCP server (`127.0.0.1:55555`) i **automatski pokreće** `MeteringSimulator.exe` (pronalazi ga u build izlazu susednog projekta). Simulator pita koliko entiteta postoji i potom u nasumičnim trenucima šalje merenja. Posle svakog dodavanja/brisanja entiteta simulator se **automatski restartuje** da bi ponovo pročitao broj objekata.

> Ako iz nekog razloga simulator ne krene automatski, pokrenite `MeteringSimulator.exe` ručno iz njegovog `bin` foldera — povezaće se sam.

## Komunikacija (protokol)

Tekstualni protokol, jedna poruka po liniji:

```
Simulator → Servis :  Object count?
Servis    → Simulator :  3
Simulator → Servis :  Object_2:317.45     (Object_<indeks>:<vrednost>)
```

`<indeks>` odgovara poziciji entiteta u listi koju drži `NetworkService`. Svako primljeno merenje se upisuje u **`log.txt`** (u izlaznom folderu aplikacije), uz vremenski trenutak, referencu na entitet i vrednost.

## Arhitektura (MVVM)

```
NetworkService/
├─ Models/        Entity, EntityType, MeasurementRecord
├─ MVVM/          ObservableObject (INotifyPropertyChanged), RelayCommand
├─ Undo/          IUndoableAction, RelayUndoableAction, UndoManager (stek istorije)
├─ Services/      LogService, SimulatorServer (TCP + proces simulatora),
│                 ToastService, ConfirmationService, EntityTypeCatalog, SeedData
├─ ViewModels/    MainViewModel (koordinator + navigacija + Home/Undo),
│                 Home, NetworkEntities (P1), NetworkDisplay (Drag&Drop),
│                 MeasurementGraph (G3), AddEntity (validacija + tastatura)
├─ Views/         odgovarajući XAML prikazi
├─ Converters/    Bool→Visibility, Valid→Brush, ImageKey→DrawingImage, ToastType→Brush
└─ Resources/     Theme.xaml (paleta/fontovi/stilovi), TypeImages.xaml (vektorske ikonice)
```

Sav UI je vezan kroz **DataBinding**; navigacija se radi preko `DataTemplate`-a koji mapiraju ViewModel na View.

## Mapiranje zahteva iz specifikacije

- **Log datoteka** — `LogService` upisuje svako merenje (vreme, entitet, vrednost) u `log.txt`.
- **Network Entities View** — tabela entiteta, kolona sa poslednjom izmerenom vrednošću, **dodavanje** (forma + virtuelna tastatura) i **brisanje** (uz potvrdu), **pretraga P1**, poništavanje pretrage; po izmeni se restartuje simulator.
- **Network Display View** — 12 ćelija (canvas), **TreeView** grupisan po tipu (entitet nestaje iz stabla kad se postavi, vraća se kad se ukloni), **Drag&Drop** sa stabla na mrežu i između ćelija, **povezivanje linijama** (klik na dva entiteta), linije prate entitete pri pomeranju, brisanje/uklanjanje entiteta uklanja i njegove linije, sprečeno dupliranje veze, sadržaj se **čuva pri navigaciji** (jedna instanca ViewModel-a).
- **Measurement Graph View (G3)** — izbor entiteta preko ComboBox-a, poslednjih 5 vrednosti iz `log.txt`, **real-time** osvežavanje, validne/nevalidne vrednosti različitom bojom, **programsko** iscrtavanje.
- **Validacija** — po svakom polju pojedinačno, poruka ispod polja (bez MessageBox-a), smislene poruke prilagođene grešci.
- **Povratne informacije** — Toast notifikacije za uspešne akcije (tip, naslov, sadržaj), potvrda pri brisanju kao in-app overlay, ToolTip-ovi i promene kursora; **nigde se ne koristi MessageBox**.
- **Prečice** — `Ctrl+H` (Home), `Ctrl+1/2/3` (Entiteti / Mreža / Grafikon), `Ctrl+Z` (Undo).
- **Modelovanje (T7)** — `Entity` (ID:int jedinstven, Naziv, Tip), tipovi RTD/TermoSprega sa unapred definisanim (vektorskim) slikama; opseg 250–350 °C.

## Napomene za odbranu

- Aplikacija startuje sa **4 unapred kreirana entiteta** sa validnim podacima primerenim temi (vidi `Services/SeedData.cs`).
- Slike tipova su realizovane kao **vektorske `DrawingImage`** ikonice (`Resources/TypeImages.xaml`) — ostaju oštre na svakoj veličini i čuvaju se uz projekat; `EntityType.ImageKey` je ključ koji konverter mapira na odgovarajuću ikonicu.
- Jezik koda je dosledno **engleski**, jezik korisničkog interfejsa dosledno **srpski**.
