# Risikoanalyse

Die folgende Analyse wurde zu Beginn des Projekts durchgeführt, um potenzielle Gefahren für den Projekterfolg frühzeitig zu erkennen und proaktiv zu managen.

| ID | Risiko-Kategorie | Risiko-Beschreibung | W* | A* | Massnahme (Vermeidung/Minderung) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **R-01** | **Anforderungen** | **Unvollständige Spezifikation:** Die Grobanforderungen könnten Interpretationsspielraum lassen (z.B. Tarifdetails). | 2 | 2 | Erstellung eines detaillierten Lastenhefts und Klärung offener Fragen im ersten Review. |
| **R-02** | **Ressourcen** | **Zeitengpass:** Da es sich um eine Einzelarbeit handelt, führt ein Ausfall des Projektleiters direkt zum Stillstand. | 2 | 3 | Frühzeitiger Start der Arbeiten und Einplanung von Pufferzeiten vor den Meilensteinen. |
| **R-03** | **Technologie** | **Fehlende Erfahrung mit Simulation:** Die Anforderung, Hardware am Bildschirm zu simulieren, ist ein neues Element. | 2 | 2 | Frühzeitige Recherche nach geeigneten UI-Frameworks und Erstellung eines "Proof of Concept" für die Schrankensteuerung. |
| **R-04** | **Scope** | **"Gold Plating":** Gefahr, sich in KANN-Zusätzen zu verlieren, bevor die Kernlogik stabil läuft. | 1 | 2 | Strikte Priorisierung nach MoSCoW-Methode (Must, Should, Could, Won't) im Lastenheft. |
| **R-05** | **Qualität** | **Logikfehler in Kernalgorithmen:** Falsche Berechnungen beim Parktarif führen zu Fehlern im System. | 2 | 3 | Definition von Testfällen bereits in der Designphase und systematisches Testen der Grenzfälle. |

*\* Bewertung: 1 = niedrig, 2 = mittel, 3 = hoch*

## Erläuterung der Strategie
Die Risikoanalyse wird während des gesamten Projektverlaufs im Rahmen des Controllings überwacht. Insbesondere das Risiko **R-05 (Qualität)** wird durch eine hohe Testabdeckung der Service-Klassen (Tarif- und Parkplatz-Logik) adressiert, während **R-01 (Anforderungen)** durch die enge Abstimmung in den Reviews minimiert wird.
