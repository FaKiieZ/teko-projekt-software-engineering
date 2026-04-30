# Klassendiagramm (Struktur / Datenmodell)

## Klassendiagramm bevor Umsetzung

Das Klassendiagramm bildet die Kernanforderungen ab: Ein Parkhaus hat mehrere Stockwerke , welche wiederum aus mehreren Parkplätzen bestehen. Wir trennen sauber zwischen Dauermieter (mit Code und fixem Platz) und Gelegenheitsnutzer (mit Ticket).

```mermaid
classDiagram
    class Parkhaus {
        -int id
        -List~Stockwerk~ stockwerke
        -Schranke eingangsschranke
        -Schranke ausgangsschranke
        +konfiguriereParkhaus(stockwerke: int, plaetze: int)
        +zuweisenParkplatz() Parkplatz
        +berechneUmsatz() double
    }

    class Stockwerk {
        -int nummer
        -List~Parkplatz~ parkplaetze
    }

    class Parkplatz {
        -int nummer
        -boolean istBesetzt
        -Typ typ~Dauer, Gelegenheits~
    }

    class Benutzer {
        <<abstract>>
    }

    class Dauermieter {
        -String zugangsCode
        -Parkplatz fixerPlatz
        +einfahren(code: String) boolean
    }

    class Gelegenheitsnutzer {
        -Ticket aktuellesTicket
        +ticketZiehen() Ticket
    }

    class Ticket {
        -DateTime eingangsZeit
        -DateTime ausgangsZeit
        -int stockwerkNummer
        -int parkplatzNummer
        -boolean istBezahlt
        +berechneTarif() double
        +entwerten()
    }

    class Schranke {
        -Typ typ~Eingang, Ausgang~
        -boolean istOffen
        +oeffnen()
        +schliessen()
    }

    Parkhaus "1" *-- "1..*" Stockwerk : besteht aus
    Stockwerk "1" *-- "1..*" Parkplatz : enthält
    Parkhaus "1" *-- "2" Schranke : besitzt
    Benutzer <|-- Dauermieter
    Benutzer <|-- Gelegenheitsnutzer
    Gelegenheitsnutzer --> "1" Ticket : besitzt
    Dauermieter --> "1" Parkplatz : hat fixen
```

Erklärung für deine Doku: Dieses Diagramm zeigt auf, dass das Parkhaus logisch über genau eine Eingangsschranke und eine Ausgangsschranke verfügt. Ebenso ist ersichtlich, dass das Ticket alle geforderten Attribute (Eingangszeit, Stockwerk, Parkplatznummer) speichert , damit später der 15-Minuten-Tarif bzw. die Tagespauschale berechnet werden kann.

Ein kleiner Tipp am Rande für unser Review: Vergiss nicht, in deiner Dokumentation kurz zu begründen, warum du diese Diagramme so aufgebaut hast. Die Dozenten lieben es, wenn man Entscheidungen nachvollziehbar dokumentiert ("Wir haben uns für eine Vererbung bei den Benutzern entschieden, weil..."). Das gibt ordentlich Pluspunkte bei der Bewertung der Architektur!

## Klassendiagramm nach Umsetzung

Dieses Klassendiagramm bildet den aktuellen Stand der Implementierung (As-Built) ab. Es entspricht der Datenbankstruktur, wie sie durch Entity Framework Core (Code-First) in der SQLite-Datenbank `easyparking.db` generiert wird.

```mermaid
classDiagram
    class ParkingGarage {
        +int Id
        +String Name
        +List~Floor~ Floors
    }

    class Floor {
        +int Id
        +int Number
        +int TotalSpaces
        +List~ParkingSpace~ ParkingSpaces
        +int ParkingGarageId
    }

    class ParkingSpace {
        +int Id
        +int Number
        +boolean IsOccupied
        +int FloorId
        +int? AssignedTenantId
    }

    class Customer {
        +int Id
        +String Code
        +CustomerType Type
        +boolean IsActive
    }

    class Ticket {
        +int Id
        +DateTime EntryTime
        +DateTime? ExitTime
        +int FloorNumber
        +int SpaceNumber
        +decimal? Cost
        +boolean IsPaid
        +int? CustomerId
    }

    class Tariff {
        +int Id
        +DayType DayType
        +TimeSpan StartTime
        +TimeSpan EndTime
        +decimal RatePerHour
    }

    class CustomerType {
        <<enumeration>>
        Occasional
        Tenant
    }

    class DayType {
        <<enumeration>>
        Weekday
        Weekend
    }

    ParkingGarage "1" *-- "1..*" Floor : besteht aus
    Floor "1" *-- "1..*" ParkingSpace : enthält
    ParkingSpace "0..1" --> "0..1" Customer : reserviert für (Dauermieter)
    Ticket "0..*" --> "0..1" Customer : zugeordnet zu
    Ticket ..> Tariff : Preisberechnung via
```

## Erläuterungen für die Dokumentation

Die Modellierung wurde gegenüber dem ersten Entwurf verfeinert, um eine effiziente Persistenz und die komplexen Tarifanforderungen abzubilden:

1. **Vermeidung von Vererbung (Customer):** Anstelle einer komplexen Klassen-Hierarchie wird ein `CustomerType` verwendet. Dies vereinfacht die Abbildung in der relationalen Datenbank (Table-per-Hierarchy) und reicht für die Unterscheidung zwischen Gelegenheitsnutzern und Dauermietern vollkommen aus.
2. **Entkopplung der Tarife:** Die Klasse `Tariff` erlaubt es, die in der Aufgabenstellung geforderten Tarife (Wochentage vs. Wochenende/Feiertage) flexibel in der Datenbank zu hinterlegen und zu verwalten.
3. **Ticket-Historie:** Das `Ticket` fungiert als zentrales Protokoll-Element. Es speichert nicht nur die Zeiten (Einfahrt/Ausfahrt), sondern auch den finalen berechneten Betrag und die Stellplatz-Informationen, was für spätere statistische Auswertungen (Umsatz pro Monat/Jahr) essenziell ist.
4. **Referenzielle Integrität:** Die Relationen (z. B. `AssignedTenantId` auf einem `ParkingSpace`) stellen sicher, dass Dauermieter ihren fixen Platz behalten, während das System gleichzeitig die Belegung durch Gelegenheitsnutzer überwacht.
