# Media Ratings Platform (MRP) – Intermediate Submission  
Author: Erion Polisi

GitHub: https://github.com/erionpolisi/MRP.git

---

# 1. Projektübersicht

Dieses Projekt implementiert einen eigenständigen HTTP/REST-Server (ohne Frameworks wie ASP.NET) zur Umsetzung einer Media Ratings Platform (MRP).
Der Server stellt eine API für mögliche Frontends bereit (z. B. Web oder Mobile), welche nicht Teil dieses Projekts sind.

Die Anwendung bietet:
- User-Authentifizierung (Registrierung & Login)

- Token-basierte Autorisierung

- CRUD-Verwaltung von Media-Einträgen (Movies, Series, Games)

- Einheitliche JSON-Responses über eigene Response-Helper

Alle geschützten Endpunkte werden durch eine zentrale Session-Validierung abgesichert. Diese Abgabe erfüllt vollständig die Intermediate Submission Requirements laut Aufgabenstellung.

---

# 2. Architekturüberblick

Die Anwendung folgt einer klar modularen Architektur:

- **HttpRestServer**  

  Lauscht auf Port 8080 und nimmt eingehende HTTP-Requests an.

- **Handlers** (`UserHandler`, `MediaHandler`, `RatingHandler` , `LeaderboardHandler` , `VersionHandler`)

  Jeder Handler verarbeitet Requests basierend auf dem HTTP-Pfad.

- **Models**  
  - `User`  
  - `MediaEntry`  
  - `Rating`  
  - `Session`  

- **Repositories**  
  - `UserRepository`  
  - `MediaEntryRepository`
  - `RatingRepository`

  - `MediaFavoriteRepository`
  - `RatingLikeRepository`


Die Repositories kapseln SQL-Zugriffe auf eine PostgreSQL-Datenbank
und trennen Datenbanklogik strikt vom HTTP-Layer.

Der Server verwendet **HttpListener**, JSON-Parsing über `System.Text.Json`, ein eigenes Routing und ein eigenes Token-basiertes Session-System.

---

# 3. Token Authentication

Nach erfolgreichem Login wird ein Token generiert:

- 24 zufällige Zeichen  
- Session wird in Memory gespeichert  
- Jeder Request außer `register` und `login` erfordert einen gültigen Bearer-Token im Header.

Beispiel: Authorization: Bearer abcdef....

Sessions laufen nach **30 Minuten** automatisch ab (Cleanup-Mechanismus).

401-Responses enthalten korrekte JSON-Fehlermeldungen.

---

# 4. Designentscheidungen

### Routing  
Alle Handler werden dynamisch über Reflection geladen (Handler.LoadHandlers())
und das Routing basiert auf String-Vergleichen des HTTP-Pfads.

Alle Handler werden automatisch erkannt (`Handler.HandleEvent`).

### Authentifizierung  
Sessions werden bewusst in Memory gespeichert, weil Intermediate keine DB erfordert.

---

# 5.  SOLID-Prinzipien (mit Beispielen)  
- **S** UserHandler, MediaHandler — klare Verantwortlichkeiten  
- **O** Neue Handler können ohne Änderungen am Server hinzugefügt werden  
- **L** Models lassen sich austauschen, MediaEntry erfüllt Interface-Freiheit  
- **I** Keine übergroßen Interfaces  
- **D** Repositories und Services abstrahieren Logik sauber vom HTTP-Layer

### PERSISTENZ
- PostgreSQL als relationale Datenbank
- Repositories kapseln SQL vollständig
- Keine SQL-Logik in Handlern oder Modellen

---

# 6. Starten des Servers

dotnet run

Server startet auf: http://localhost:8080/

---

# 7. Integration Tests

Tests verwenden Postman Collection Runner, Token wird automatisch in der Laufzeit gesetzt.
Die Postman Collection im Projekt zeigt alle relevanten Intermediate-Endpunkte:

- Registrierung
- Login
- Profil abfragen
- Media CRUD
- Suchfunktion

Die Collection kann direkt importiert und ausgeführt werden.

---

# 8. Lessons Learned 

Während der Entwicklung der Media Ratings Platform habe ich mehrere wichtige technische und konzeptionelle Erkenntnisse gewonnen:

- Ein frameworkloser HTTP-Server (HttpListener) erfordert saubere Trennung von Routing, Business-Logik und Datenzugriff, da viele Komfortfunktionen fehlen.

- Token-basierte Authentifizierung ist fehleranfällig, wenn Sessions nicht konsequent vor jedem geschützten Endpoint geprüft werden.

- Eine saubere Repository-Abstraktion erleichtert spätere Änderungen an der Persistenz (z. B. Wechsel von In-Memory zu PostgreSQL).

- Der Einsatz von Atoms + Repositories mit Edit-/Verify-Logik hilft, Zugriffsrechte (Owner/Admin) zentral und sicher umzusetzen.

- Fehler in SQL (z. B. falsche Datentypen oder fehlende Constraints) äußern sich oft erst zur Laufzeit → Logging und schrittweises Testen sind essenziell.

- Kleine Designentscheidungen (z. B. wann BeginEdit() notwendig ist) haben große Auswirkungen auf Stabilität und Sicherheit.

Insgesamt hat das Projekt mein Verständnis für Backend-Architektur, REST-Design, Datenbankzugriffe und Fehleranalyse deutlich verbessert.

---

# 9. Unit Testing Strategy and Coverage

Die Tests konzentrieren sich auf die Kern-Business-Logik, nicht auf den HTTP-Transport selbst.

Teststrategie:

- Fokus auf Repositories und Domain-Logik (User, Media, Rating, Favorites)
- Prüfung von:
  - Erstellen, Lesen, Aktualisieren und Löschen von Entitäten
  - Berechtigungslogik (Owner/Admin-Prüfungen)
  - Rating-Logik (Stars 1–5, Kommentar, Confirmation)
  - Like- / Unlike-Funktionalität

- Negative Tests:
  - Ungültige IDs
  - Fehlende Session
  - Unberechtigter Zugriff

Coverage:
Kernlogik vollständig getestet
HTTP-Handler werden indirekt über Postman getestet
-Datenbankinteraktionen werden über reale PostgreSQL-Verbindungen geprüft

Die Kombination aus Unit Tests + Postman Integration Tests stellt sicher, dass sowohl Business-Logik als auch API-Verhalten korrekt funktionieren.

---

# 10. Tracked Time for Major Tasks (Zeitaufwand)

- Projektsetup & Grundarchitektur	6 h
- HTTP-Server & Routing	8 h
- User-Registrierung, Login, Sessions	7 h
- Media CRUD + Suche & Filter	9 h
- Rating-System (Create, Edit, Confirm)	10 h
- Likes & Favorites	6 h
- Empfehlungen (Genre & Content)	6 h
- PostgreSQL-Integration	7 h
- Debugging & Fehlerbehebung	8 h
- Dokumentation & Protokoll	4 h

Gesamtaufwand: ca. 71 Stunden

# 11. Fazit

Die Final Submission der Media Ratings Platform (MRP) erfüllt sämtliche Anforderungen der Aufgabenstellung vollständig:  
✔ funktionierender HTTP-Server  
✔ Routing  
✔ JSON-basierte Requests  
✔ token-basierte Authentifizierung  
✔ User & Media CRUD  
✔ Postman Collection  
✔ SOLID  
✔ Protokoll  

✔ eigenständiger HTTP/REST-Server ohne Frameworks (HttpListener)
✔ token-basierte Authentifizierung mit Session-Verwaltung
✔ vollständiges User-Profil inkl. Statistiken (Ratings, Favorites, Empfehlungen)
✔ Media-Management mit CRUD, Such-, Filter- und Sortierfunktionen
✔ Rating-System mit Bearbeiten, Löschen, Likes und Kommentar-Bestätigung
✔ Favoriten-System für Media-Einträge
✔ Empfehlungslogik (Genre- und Content-basierte Empfehlungen)
✔ öffentliches Leaderboard der aktivsten User
✔ PostgreSQL-Persistenz über Repository-Pattern
✔ saubere Trennung von HTTP-Layer, Business-Logik und Datenzugriff
✔ vollständige Postman-Collection für alle Endpunkte
✔ Protokoll mit Architektur-, Design- und Entwicklungsentscheidungen

Die Anwendung ist stabil, erweiterbar und folgt klaren Architektur- und SOLID-Prinzipien.
Alle Must-Haves der Final Submission wurden umgesetzt und getestet.
Das Projekt ist bereit für die Live-Demonstration, Bewertung und Weiterentwicklung.



