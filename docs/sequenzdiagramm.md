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
    participant Kasse as Kassenautomat (GUI)
    participant KA as TarifRechner
    participant ZS as Zahlsystem <<Schnittstelle>>
    participant T as Ticket
    participant Ausfahrt as Ausfahrtsterminal (GUI)
    participant S as Ausgangsschranke

    Note over GN, S: Phase 1: Bezahlung am Automaten
    GN->>Kasse: Ticketnummer eingeben / scannen
    activate Kasse
    Kasse->>T: getEingangsZeit()
    T-->>Kasse: Zeitstempel
    Kasse->>KA: berechneTarif(eintritt, austritt)
    Note right of KA: Logik: 15-Min-Takt & 24h-Pauschale
    KA-->>Kasse: Geschuldeter Betrag

    Kasse->>ZS: autorisiereZahlung(Betrag)
    activate ZS
    ZS-->>Kasse: Zahlung erfolgreich
    deactivate ZS

    Kasse->>T: setBezahlt(true)
    Kasse->>T: setAusgangsZeit(aktuelleZeit)
    Kasse-->>GN: Austrittsticket ausgeben (visuell)
    deactivate Kasse

    Note over GN, S: Phase 2: Ausfahrt an der Schranke
    GN->>Ausfahrt: Austrittsticket einführen
    activate Ausfahrt
    Ausfahrt->>T: istBezahlt()

    alt Ticket ist bezahlt
        T-->>Ausfahrt: true
        Ausfahrt->>S: oeffnen()
        S-->>Ausfahrt: Status: Offen
        Ausfahrt-->>GN: Anzeige: "Gute Fahrt"
        Ausfahrt->>S: schliessen()
    else Ticket nicht bezahlt
        T-->>Ausfahrt: false
        Ausfahrt-->>GN: Fehler: "Bitte am Automaten nachzahlen" (NFA-20.1)
    end
    deactivate Ausfahrt
```

### Logik-Check: Der Tarif-Rechner (FA-40)

Die Anforderungen FA-40.6 bis FA-40.10 sind knifflig zu programmieren: Du musst im 15-Minuten-Takt abrechnen (), angebrochene Viertelstunden voll verrechnen () und bei über 24 Stunden auf die Tagespauschale von CHF 35.00 wechseln ().Damit du deine Algorithmen in der Phase "Kernfunktionen" (ab dem 20.04. laut Zeitplan) direkt überprüfen kannst, habe ich dir diesen Rechner erstellt:Der Rechner hilft dir, die Logik für dein Pflichtenheft präzise zu beschreiben und sicherzustellen, dass die Rundungsregeln korrekt angewendet werden.Es hilft auch dabei, die Anforderungen FA-40.6 bis FA-40.10 zu verstehen und im Prototyp umzusetzen.Tipps für die Dokumentation (nach ):Begründung der Schnittstelle: Erwähne in der Projektdokumentation explizit, dass das Zahlsystem eine externe Schnittstelle ist, um die Komplexität des Prototyps gering zu halten. Das zeigt, dass du "Systemabgrenzung" verstanden hast.Fehlerbehandlung: Ergänze im Sequenzdiagramm oder im Text, was passiert, wenn das Ticket nicht bezahlt ist (NFA-20.1: System darf nicht abstürzen, sondern muss Fehlermeldung ausgeben ).Passt das so für dein Design-Kapitel, oder brauchst du noch ein Zustandsdiagramm (State Chart) für den Status eines Parkplatzes (frei/besetzt/reserviert)?
