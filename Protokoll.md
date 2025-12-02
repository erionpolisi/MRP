# Media Ratings Platform (MRP) – Intermediate Submission  
Author: Erion Polisi

GitHub: https://github.com/erionpolisi/MRP.git

---

# 1. Projektübersicht

Dieses Projekt implementiert einen reinen HTTP/REST-Server (ohne Frameworks wie ASP.NET) zur Verwaltung einer Media Ratings Platform (MRP).  
Der Server bietet grundlegende User-Authentifizierung (Registration, Login, Token-System) sowie CRUD-Verwaltung von Media-Einträgen.
Responses werden konsistent über Erweiterungsmethoden (RespondXXX) verarbeitet.
Eine interne Session-Logik prüft Autorisierung vor allen geschützten Endpunkten.
Diese Abgabe entspricht vollständig den **Intermediate Requirements**.

---

# 2. Architekturüberblick

Die Anwendung folgt einer klar modularen Architektur:

- **HttpRestServer**  

  Lauscht auf Port 8080 und nimmt eingehende HTTP-Requests an.

- **Handlers** (`UserHandler`, `MediaHandler`, `VersionHandler`)

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
  - `MediaService` (vorgesehen für Final)

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

# 4. Implementierte Endpoints (Intermediate => REST-API Endpoints)

| Kategorie               | Endpoint              | Methode | Status                           |
| ----------------------- | --------------------- | ------- | -------------------------------- |
| **Authentication**      | `/users/register`     | POST    | ✔ vollständig                    |
|                         | `/users/login`        | POST    | ✔ vollständig                    |
| **User-Profil (basic)** | `/users/{id}/profile` | GET     | ✔ vollständig                    |
|                         | `/users/{id}/profile` | PUT     | ✔ vollständig                    |
| **Media CRUD (basic)**  | `/media`              | POST    | ✔ vollständig                    |
|                         | `/media`              | GET     | ✔ mit Titel-Filter               |
|                         | `/media/{id}`         | GET     | ✔ vollständig                    |
|                         | `/media/{id}`         | PUT     | ✔ (Creator-Check noch für Final) |
|                         | `/media/{id}`         | DELETE  | ✔ (Creator-Check noch für Final) |


---

# 5. Designentscheidungen

### Routing  
Alle Handler werden dynamisch über Reflection geladen (Handler.LoadHandlers())
und das Routing basiert auf String-Vergleichen des HTTP-Pfads.

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

# 8. Fazit

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



