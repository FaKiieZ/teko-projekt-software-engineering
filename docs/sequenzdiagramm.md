# Sequenzdiagramme

## (Dynamik: Einfahrt Gelegenheitsnutzer)

Um dem Dozenten zu zeigen, wie dein System in Aktion funktioniert (und wie die Objekte miteinander interagieren), ist ein Sequenzdiagramm für den "First-Free-Spot"-Algorithmus ideal. Es simuliert die Hardware-Komponenten (Ticketautomat/Schranke) am Bildschirm.

```mermaid
sequenceDiagram
    actor GN as Gelegenheitsnutzer
    participant UI as GUI (Simulation)
    participant PH as Parkhaus
    participant T as Ticket

    GN->>UI: Klickt "Ticket ziehen"
    activate UI
    UI->>PH: zuweisenParkplatz()
    activate PH

    alt Freier Platz vorhanden
        PH-->>UI: Rückgabe: Parkplatz (Stockwerk, Nummer)
        UI->>T: create(aktuelleZeit, Stockwerk, Nummer)
        activate T
        T-->>UI: Ticket-Objekt erstellt
        deactivate T
        UI->>UI: Simuliere: Öffne Eingangsschranke
        UI-->>GN: Zeige Ticket visuell an
    else Kein Platz vorhanden
        PH-->>UI: Rückgabe: null
        UI-->>GN: Zeige Fehlermeldung "Parkhaus besetzt"
    end

    deactivate PH
    deactivate UI
```

Erklärung für deine Doku:
Dieses Sequenzdiagramm dokumentiert den Ablauf von Anforderung FA-30.1 (automatisierte Zuweisung) und FA-60.4 (visuelle Darstellung des Tickets auf dem Bildschirm). Es zeigt deutlich, dass der Gelegenheitsnutzer nur dann ein Ticket erhält, sofern ein freier Platz verfügbar ist.

## Bezahlung & Ausfahrt

Dieses Diagramm zeigt den Ablauf für einen Gelegenheitsnutzer. Es berücksichtigt die Anforderung, dass das Ticket entwertet wird (), die Hardware simuliert wird () und das Zahlsystem nur über eine Schnittstelle angebunden ist ().

```mermaid
sequenceDiagram
    actor GN as Gelegenheitsnutzer
    participant UI as GUI (Simulation)
    participant KA as Kassenlogik
    participant T as Ticket
    participant ZS as Zahlsystem (Schnittstelle)
    participant S as Ausgangsschranke

    Note over GN, S: Prozess: Bezahlung am Automaten
    GN->>UI: Ticket in "Automaten" einführen
    UI->>T: getEingangsZeit()
    T-->>UI: Zeitstempel
    UI->>KA: berechneTarif(eintritt, austritt)
    Note right of KA: Logik: 15-Min-Takt & 24h-Pauschale
    KA-->>UI: Betrag (z.B. CHF 12.50)

    UI->>ZS: autorisiereZahlung(Betrag)
    activate ZS
    ZS-->>UI: Zahlung erfolgreich
    deactivate ZS

    UI->>T: setBezahlt(true)
    UI->>T: entwerten(aktuelleZeit)
    UI-->>GN: Ticket ausgeben (visuell simuliert)

    Note over GN, S: Prozess: Ausfahrt an der Schranke
    GN->>UI: Ticket an Schranke scannen
    UI->>T: istBezahlt()
    T-->>UI: true
    UI->>S: oeffnen()
    S-->>UI: Status: Offen
    UI-->>GN: Anzeige: "Gute Fahrt"
    UI->>S: schliessen()
```

### Logik-Check: Der Tarif-Rechner (FA-40)

Die Anforderungen FA-40.6 bis FA-40.10 sind knifflig zu programmieren: Du musst im 15-Minuten-Takt abrechnen (), angebrochene Viertelstunden voll verrechnen () und bei über 24 Stunden auf die Tagespauschale von CHF 35.00 wechseln ().Damit du deine Algorithmen in der Phase "Kernfunktionen" (ab dem 20.04. laut Zeitplan) direkt überprüfen kannst, habe ich dir diesen Rechner erstellt:Der Rechner hilft dir, die Logik für dein Pflichtenheft präzise zu beschreiben und sicherzustellen, dass die Rundungsregeln korrekt angewendet werden.Es hilft auch dabei, die Anforderungen FA-40.6 bis FA-40.10 zu verstehen und im Prototyp umzusetzen.Tipps für die Dokumentation (nach ):Begründung der Schnittstelle: Erwähne in der Projektdokumentation explizit, dass das Zahlsystem eine externe Schnittstelle ist, um die Komplexität des Prototyps gering zu halten. Das zeigt, dass du "Systemabgrenzung" verstanden hast.Fehlerbehandlung: Ergänze im Sequenzdiagramm oder im Text, was passiert, wenn das Ticket nicht bezahlt ist (NFA-20.1: System darf nicht abstürzen, sondern muss Fehlermeldung ausgeben ).Passt das so für dein Design-Kapitel, oder brauchst du noch ein Zustandsdiagramm (State Chart) für den Status eines Parkplatzes (frei/besetzt/reserviert)?
