# Klassendiagramm (Struktur / Datenmodell)

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
