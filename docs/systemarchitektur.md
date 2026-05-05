## 2.9 Systemarchitektur

In diesem Kapitel wird die gewählte Softwarearchitektur der «EasyParking»-Anwendung beschrieben. Das System wurde so entworfen, dass es die Anforderungen an Wartbarkeit, Testbarkeit und Erweiterbarkeit erfüllt, während es gleichzeitig die im Variantenentscheid (Kapitel 1.8.3) gewählte hybride Edge-Architektur logisch abbildet.

### 2.9.1 Architektur-Überblick und Entwurfsmuster

Die Anwendung basiert auf dem **Model-View-ViewModel (MVVM)** Entwurfsmuster. Dieses Muster ist der Industriestandard für moderne WPF-Anwendungen und ermöglicht eine strikte Trennung zwischen der Benutzeroberfläche und der Geschäftslogik.

*   **View (Präsentationsschicht):** Die Views (z. B. `MainWindow.xaml`) sind ausschliesslich für die visuelle Darstellung und das Benutzer-Feedback zuständig. Sie enthalten keine Geschäftslogik. Die Bindung an die Daten erfolgt über das WPF-Data-Binding.
*   **ViewModel (Logikschicht):** Das `MainViewModel` fungiert als Bindeglied. Es hält den Zustand der View, verarbeitet Benutzereingaben über Commands und delegiert komplexe Aufgaben an spezialisierte Services. Durch den Einsatz des `CommunityToolkit.Mvvm` wird eine saubere und effiziente Implementierung des `INotifyPropertyChanged`-Interfaces sichergestellt.
*   **Model (Datenschicht):** Die Models repräsentieren die reinen Datenstrukturen (Entities) und den Datenbankkontext. Sie sind unabhängig von der Darstellung und der Geschäftslogik.

### 2.9.2 Schichtenmodell (Layered Architecture)

Zusätzlich zum MVVM-Muster wird innerhalb der Logikschicht eine Service-orientierte Architektur angewendet, um die Verantwortlichkeiten klar zu trennen (Separation of Concerns):

1.  **UI-Layer (Views):** WPF-Oberfläche für die Simulation und Administration.
2.  **Application-Layer (ViewModels):** Steuerung der UI-Logik und Orchestrierung der Services.
3.  **Domain-Layer (Services):** Hier ist die "Core-Logik" gekapselt.
    *   `ParkingService`: Verantwortlich für die Parkplatz-Zuteilungsalgorithmen (z. B. "First-Free-Spot") und die Validierung von Belegungen.
    *   `TariffService`: Berechnet die Parkgebühren basierend auf den hinterlegten Tarifmodellen und Zeiträumen.
4.  **Data-Access-Layer (Models & EF Core):** Verantwortlich für die Persistenz der Daten.

### 2.9.3 Kommunikation und Datenfluss

Da es sich um einen Prototyp handelt, der eine **hybride Edge-Architektur** simuliert, ist die Software bereits für eine verteilte Umgebung vorbereitet:

*   **Lokale Autonomie:** Der Prototyp arbeitet vollständig lokal (Edge-Logik), was eine sofortige Schrankensteuerung ohne Netzwerklatenz ermöglicht.
*   **Datenkonsistenz:** Die Geschäftslogik ist so in Services gekapselt, dass sie in einer späteren Ausbaustufe problemlos zwischen einem lokalen Client und einem Zentralserver (Cloud) aufgeteilt werden kann.
*   **Service-Injection:** Die Trennung von Services und ViewModels ermöglicht es, die Logik-Komponenten einfach auszutauschen oder automatisiert mit Unit-Tests (siehe Kapitel 3.5) zu prüfen, ohne die grafische Oberfläche starten zu müssen.

Diese modulare Architektur stellt sicher, dass «EasyParking» nicht nur als Prototyp funktioniert, sondern eine solide Basis für eine produktive, skalierbare Lösung bietet.
