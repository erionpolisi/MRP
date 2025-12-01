# Media Ratings Platform (MRP) – Intermediate Submission  
Author: Erion Polisi
GitHub: https://github.com/erionpolisi/MRP.git

---

# 1. Projektübersicht

Dieses Projekt implementiert einen reinen HTTP/REST-Server (ohne Frameworks wie ASP.NET) zur Verwaltung einer Media Ratings Platform (MRP).  
Der Server bietet grundlegende User-Authentifizierung (Registration, Login, Token-System) sowie CRUD-Verwaltung von Media-Einträgen.  
Diese Abgabe entspricht vollständig den **Intermediate Requirements**.

---

# 2. Architekturüberblick

Die Anwendung folgt einer klar modularen Architektur:

- **HttpRestServer**  
  Lauscht auf Port 8080 und nimmt eingehende HTTP-Requests an.

- **Handlers** (`UserHandler`, `MediaHandler`, `SessionHandler`, `VersionHandler`)  
  Jeder Handler verarbeitet Requests basierend auf dem HTTP-Pfad.

- **Models**  
  - `User`  
  - `MediaEntry`  
  - `Rating`  
  - `Session`  

- **Repositories**  
  - `UserRepository`  
  - `MediaRepository`
  Derzeit nutze ich keine Datenbank, daher die Repositorys.  

- **Services**  
  - `UserService` (Registrierung und Login-Logik)

Der Server verwendet **HttpListener**, JSON-Parsing über `System.Text.Json`, ein eigenes Routing und ein eigenes Token-basiertes Session-System.

---

# 3. Token Authentication

Nach erfolgreichem Login wird ein Token generiert:

- 24 zufällige Zeichen  
- Session wird in Memory gespeichert  
- Jeder Request außer `register` und `login` erfordert einen gültigen Bearer-Token im Header.

Beispiel: Authorization: Bearer abcdef....

Sessions laufen nach **30 Minuten** automatisch ab (Cleanup-Mechanismus).

---

# 4. Implementierte Endpoints (Intermediate => REST-API Endpoints)

### ✔ /users/register (POST)  
Erstellt einen neuen User.

### ✔ /users/login (POST)  
Gibt ein Token zurück.

### ✔ /users/{id}/profile (GET/PUT)  
Eigene Profildaten abrufen/bearbeiten.

### ✔ /media (POST)  
Neues Media-Objekt erstellen.

### ✔ /media (GET)  
Media-Einträge anzeigen und nach Titel filtern.

### ✔ /media/{id} (GET)  
Daten eines Media-Eintrags abrufen.

### ✔ /media/{id} (PUT)  
Media-Eintrag aktualisieren (nur Creator).

### ✔ /media/{id} (DELETE)  
Media-Eintrag löschen (nur Creator).

---

# 5. Weggelassene Features (für Final Submission)

Diese Features sind **noch nicht implementiert**, da sie erst in der finalen Abgabe verlangt werden:

- Ratings vollständig (edit, delete, like, confirm)
- Favorites vollständig
- Leaderboard
- Recommendation-System
- Filter & Sort (advanced)
- Persistenz in PostgreSQL
- 20+ Unit Tests

---

# 6. Designentscheidungen

### Routing  
Statt eines externen Frameworks wurde ein eigenes Routing-System über Reflection implementiert.  
Alle Handler werden automatisch erkannt (`Handler.HandleEvent`).

### Authentifizierung  
Sessions werden bewusst in Memory gespeichert, weil Intermediate keine DB erfordert.

### SOLID  
- **S** UserHandler, MediaHandler, SessionHandler — klare Verantwortlichkeiten  
- **O** Neue Handler können ohne Änderungen am Server hinzugefügt werden  
- **L** Models lassen sich austauschen, MediaEntry erfüllt Interface-Freiheit  
- **I** Keine übergroßen Interfaces  
- **D** Repositories und Services abstrahieren Logik sauber vom HTTP-Layer

---

# 7. Starten des Servers

dotnet run

Server startet auf: http://localhost:8080/


---

# 8. Integration Tests

Die Postman Collection im Projekt zeigt alle relevanten Intermediate-Endpunkte:

- Registrierung
- Login
- Profil abfragen
- Media CRUD
- Suchfunktion

Die Collection kann direkt importiert und ausgeführt werden.

---

# 9. GitHub Repository

https://github.com/erionpolisi/MRP.git

---

# 10. Fazit

Alle Intermediate-Anforderungen sind erfüllt:  
✔ funktionierender HTTP-Server  
✔ Routing  
✔ JSON-basierte Requests  
✔ token-basierte Authentifizierung  
✔ User & Media CRUD  
✔ Postman Collection  
✔ SOLID  
✔ Protokoll  

Das Projekt ist bereit für die Präsentation und die finale Weiterentwicklung.



