# 3 Realisierung

In diesem Kapitel wird die technische Umsetzung des Prototyps «EasyParking» detailliert beschrieben. Der Fokus liegt dabei auf der Wahl der Werkzeuge, der Architekturimplementierung und der Realisierung der Kernfunktionen.

## 3.1 Programmierumgebung / Programmierrichtlinien

Der Prototyp wurde in einer modernen Entwicklungsumgebung erstellt, um eine hohe Codequalität und effiziente Wartbarkeit zu gewährleisten.

*   **Entwicklungsplattform:** .NET 10.0 (C#)
*   **IDE:** Microsoft Visual Studio 2022 / JetBrains Rider
*   **Framework:** Windows Presentation Foundation (WPF) für eine robuste Desktop-Oberfläche.
*   **Versionsverwaltung:** Git (gehostet auf GitHub/Azure DevOps) zur Nachverfolgbarkeit aller Änderungen.

**Programmierrichtlinien:**
Um die Lesbarkeit und Konsistenz des Codes sicherzustellen, wurden die offiziellen Microsoft C# Coding Conventions angewendet. Dies beinhaltet unter anderem die Verwendung von PascalCase für Methoden und Klassen, camelCase für lokale Variablen sowie die konsequente Nutzung von asynchroner Programmierung (`async/await`) zur Vermeidung von UI-Blockaden.

## 3.2 Softwareaufbau

Der Softwareaufbau folgt dem in Kapitel 2.9 beschriebenen MVVM-Muster. Zur Steigerung der Produktivität und Reduzierung von Boilerplate-Code wurde das **CommunityToolkit.Mvvm** eingesetzt. Dieses ermöglicht durch Source Generators die automatische Erstellung von Properties (`[ObservableProperty]`) und Commands (`[RelayCommand]`), was den Code schlanker und weniger fehleranfällig macht.

Die Geschäftslogik ist strikt in Services gekapselt:
*   **ParkingService:** Beinhaltet die Algorithmen zur Platzsuche.
*   **TariffService:** Beinhaltet die gesamte Preislogik inklusive Zeitberechnung.

## 3.3 GUI-Implementierung

Die Benutzeroberfläche wurde mit **XAML** (eXtensible Application Markup Language) erstellt. 

*   **Design-System:** Um ein modernes und ansprechendes Erscheinungsbild zu erzielen, wurde das **Material Design In XAML Toolkit** integriert. Dies bietet vorgefertigte Steuerelemente und Styles, die dem Google Material Design Standard entsprechen.
*   **Responsive Layout:** Durch den Einsatz von Grids und StackPanels ist die Oberfläche skalierbar und passt sich verschiedenen Bildschirmgrössen an.
*   **Datenbindung:** Die gesamte Kommunikation zwischen UI und Logik erfolgt über deklaratives Data-Binding im XAML, wodurch auf "Code-Behind" in den Views weitestgehend verzichtet werden konnte.

## 3.4 Datenbankimplementierung und –anbindung

Für die Persistenzschicht wurden Technologien gewählt, die speziell die Anforderungen an einen portablen Prototypen unterstützen:

*   **SQLite:** Als Datenbank-Engine kommt SQLite zum Einsatz. Da SQLite eine dateibasierte Datenbank ist, erfordert sie keine Installation eines Datenbankservers (wie SQL Server oder MySQL). Die gesamte Datenbank befindet sich in einer einzigen Datei (`easyparking.db`), was den Prototyp extrem leichtgewichtig und einfach zu verteilen macht.
*   **Entity Framework Core (EF Core):** Als Object-Relational Mapper (ORM) wurde EF Core verwendet. Der **Code-First-Ansatz** ermöglichte es, das Datenmodell direkt in C#-Klassen zu definieren. EF Core übernimmt automatisch die Erstellung des Datenbankschemas (Migrations) und ermöglicht typsichere Abfragen über LINQ.
*   **Repository-Pattern:** Der Zugriff auf die Daten erfolgt abstrahiert über den `EasyParkingDbContext`, was die Testbarkeit des Systems (z.B. durch In-Memory-Datenbanken in Unit-Tests) massiv verbessert.

## 3.5 Testprotokoll

Die Validierung der Implementierung erfolgte gemäss dem Testkonzept in Kapitel 2.10. 
*   **Automatisierte Tests:** Alle Kernservices wurden mit xUnit-Tests auf ihre korrekte Funktion geprüft.
*   **Manuelle Abnahme:** Die GUI-Funktionen wurden anhand der definierten Use-Cases (Kapitel 2.4.2) manuell verifiziert.
Das detaillierte Testprotokoll, welches die erfolgreiche Erfüllung aller Muss-Kriterien belegt, ist als separater Bericht im Anhang dieser Dokumentation beigefügt.

## 3.6 Einführung

Da es sich bei dem vorliegenden Projekt um einen funktionalen Prototyp handelt, ist eine produktive Einführung zum jetzigen Zeitpunkt nicht vorgesehen. Die Software dient als Entscheidungsgrundlage für die Geschäftsleitung der EasyParking AG. Ein Rollout-Plan für die finale Software wurde jedoch bereits konzeptionell im Einführungskonzept (Kapitel 2.11) erarbeitet.
