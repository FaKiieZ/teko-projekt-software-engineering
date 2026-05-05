## 2.10 Testkonzept

In diesem Kapitel wird beschrieben, wie die Qualität der «EasyParking»-Software systematisch sichergestellt wird. Das primäre Ziel des Testkonzepts ist die Verifizierung, dass alle im Lastenheft definierten funktionalen Anforderungen (FA) und nicht-funktionalen Anforderungen (NFA) korrekt umgesetzt wurden.

### 2.10.1 Teststrategie und Vorgehen

Aufgrund des Prototyping-Ansatzes wird eine entwicklungsbegleitende Teststrategie verfolgt. Dabei wird ein mehrstufiger Testprozess angewendet:

1.  **Unit-Testing (Komponententests):**
    *   **Fokus:** Isolierte Prüfung der Kernalgorithmen in der Logikschicht.
    *   **Werkzeuge:** xUnit Framework in Kombination mit *Entity Framework Core In-Memory Database* für schnelle und isolierte Tests ohne physische Datenbankabhängigkeit.
    *   **Umfang:** Insbesondere die komplexe Tarifberechnung (`TariffService`) und die Parkplatzvergabe (`ParkingService`) werden durch automatisierte Testreihen abgesichert.

2.  **Integrationstests:**
    *   **Fokus:** Überprüfung des Zusammenspiels zwischen den Komponenten (ViewModel, Services und Datenbank).
    *   **Umfang:** Validierung des Datenflusses vom UI-Event bis zur permanenten Speicherung in der SQLite-Datenbank.

3.  **System- und Abnahmetests (Manuelle Simulation):**
    *   **Fokus:** Prüfung der End-to-End-Geschäftsprozesse aus Sicht des Endbenutzers.
    *   **Vorgehen:** Systematische Durchführung von Testfällen über die grafische Benutzeroberfläche des Prototyps. Hierbei wird die integrierte Zeit-Simulation genutzt, um auch zeitkritische Szenarien (z. B. Tarifwechsel um Mitternacht oder Ablauf der Kulanzzeit) effizient zu validieren.

### 2.10.2 Testobjekte und Testfälle

Die nachfolgende Tabelle gibt einen Überblick über die kritischsten Testobjekte, die im Rahmen der Qualitätssicherung geprüft werden:

| Test-ID | Testobjekt / Szenario | Prüfziel | Referenz |
| :--- | :--- | :--- | :--- |
| **TF-01** | Tariflogik: Viertelstunden-Takt | Korrekte Aufrundung angebrochener Intervalle. | FA-40.6 |
| **TF-02** | Tariflogik: Tagesmaximum | Deckelung der Kosten bei 35.00 CHF pro 24h. | FA-40.9 |
| **TF-03** | Parkplatzvergabe (Balanced) | Zuweisung zum Stockwerk mit geringster Auslastung. | FA-30.2 |
| **TF-04** | Dauermieter-Zutritt | Validierung des Codes und Prüfung des Zahlungsstatus. | FA-20.2 |
| **TF-05** | Kulanzzeit (15 Min.) | Automatische Sperrung der Ausfahrt nach Zeitablauf. | FA-20.5 |

### 2.10.3 Testumgebung

Die Tests werden in einer kontrollierten Entwicklungsumgebung durchgeführt. Da es sich um einen Prototyp handelt, werden externe Hardware-Komponenten (Schranken, Ticketdrucker) softwareseitig simuliert. Dies ermöglicht eine vollständige Abdeckung der Logik, ohne dass physische Testgeräte erforderlich sind. Das detaillierte Testprotokoll mit den Ergebnissen der Durchführung befindet sich im Anhang der Dokumentation (Kapitel 3.5).
