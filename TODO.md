# Projektplan — MRP System (Intermediate + Final)

## **0. Vorbereitung (sofort)**

- [ ] Git-Repository initialisieren  
- [ ] `README.md` anlegen  
- [ ] Sprache festlegen → **C# Console App mit HttpListener**  
- [ ] PostgreSQL lokal oder via Docker setup  
- [ ] DB `mrpdb` + User erstellen  
- [ ] SQL-Migrationsdatei anlegen: `migrations/init.sql`  

---

## **1. Intermediate Requirements (Abgabe 1 — MUST HAVES)**

### **A. Minimal funktionaler HTTP Server**

#### **Projektstruktur**
- [ ] Console App (C#)  
- [ ] HttpListener auf `http://localhost:8080/`  
- [ ] In-memory Storage (`Dictionary`)  
- [ ] Repository-Schicht vorbereiten (noch ohne DB)  

---

### **Endpoints**

#### **User**
- [ ] `POST /api/users/register`  
  - Registriert Nutzer (`username`, `password`)  
- [ ] `POST /api/users/login`  
  - Gibt Token zurück  

#### **Media CRUD**
- [ ] `POST /api/media` — Create *(auth required)*  
- [ ] `GET /api/media/{id}` — Read *(public)*  
- [ ] `PUT /api/media/{id}` — Update *(only creator)*  
- [ ] `DELETE /api/media/{id}` — Delete *(only creator)*  

#### **Auth**
- [ ] Token-basierte Authorisierung (Bearer Token)  
- [ ] Middleware / Helper für Token-Validierung  

---

### **Tests / Tools**
- [ ] Testprojekt anlegen  
- [ ] Erste Unit Tests (z. B. Auth, Register/Login)  
- [ ] curl-Skripte oder Postman-Collection erstellen  
- [ ] Minimaler Architektur-Text: `protocol.md`  

---

### **B. Deliverables für Intermediate**
- [ ] ZIP mit Source Code  
- [ ] README mit Link zum Git  
- [ ] Postman/curl  
- [ ] `protocol.md`  
- [ ] Checkliste erfüllt:  
  - C#  
  - Listener funktioniert  
  - Builds successfully  
  - Nutzt manuelle HTTP-Parsing-Methoden  

---

## **2. Final Requirements (Abgabe 2 — volle Funktionalität)**

### **Persistenz**
- [ ] PostgreSQL Integration (Npgsql)  
- [ ] Migrationen vollständig (`migrations/*.sql`)  
- [ ] Repositories implementieren:
  - UserRepository  
  - MediaRepository  
  - RatingRepository  
  - TokenRepository  

---

### **User Features**
- [ ] GET `/api/users/{username}/profile`  
- [ ] PUT `/api/users/{username}/profile`  
- [ ] Token-Expiry + Logout (Token invalidieren)  
- [ ] Password hashing (bcrypt oder Argon2)  

---

### **Media Features**
- [ ] Vollständiges Media-Modell:  
  - title, desc, type, year, genres[], ageRestriction  
  - creatorId  
  - avgScore  
- [ ] Filtering & Sorting Endpoints (Query Params):  
  - genre  
  - type  
  - year  
  - ageRestriction  
  - rating  
  - Sort by: *title*, *year*, *score*  
- [ ] Search (partial title)  

---

### **Rating System**
- [ ] `POST /api/media/{id}/ratings` (1–5 + optional comment)  
- [ ] Kommentar öffentlich machen erst nach Autorenbestätigung  
- [ ] `PUT /api/media/{id}/ratings/{rid}`  
- [ ] `DELETE /api/media/{id}/ratings/{rid}`  
- [ ] Likes (1 User → 1 Like pro Rating)  
- [ ] Avg Score transactional updaten  

---

### **Favorites**
- [ ] `POST /api/media/{id}/favorite`  
- [ ] `DELETE /api/media/{id}/favorite`  
- [ ] `GET /api/users/{username}/favorites`  

---

### **Recommendations**
- [ ] Algorithmus:  
  - Hole Genres der ≥4-Star Ratings  
  - Finde Media mit gleichen Genres, Typ, AgeRestriction  
  - Sort by similarity + avgScore  

---

### **Leaderboard & Stats**
- [ ] GET `/api/leaderboard` (sort by #ratings)  
- [ ] User Stats:  
  - totalRatings  
  - avgScoreGiven  
  - favoriteGenre  

---

### **Integration Tests**
- [ ] Postman Collection für alle kompletten Flows  
- [ ] CRUD → Ratings → Favorites → Recommendations  

---

### **Unit Tests (20+)**
- [ ] Avg calculation  
- [ ] Filtering  
- [ ] Permission checks  
- [ ] Auth  
- [ ] Recommendation  
- [ ] Moderation/Comment approval  

---

### **Docs**
- [ ] `protocol.md`  
  - Architektur  
  - Tests  
  - Timeline  
  - Probleme & Lösungen  

---

### **Deployment / Extras**
- [ ] docker-compose mit Postgres (+ optional App-Container)  
- [ ] GitHub Actions (CI + Tests)  
- [ ] Logging  
- [ ] Input-Validierung  
- [ ] Rate Limiting (optional)  

---

## **3. Empfohlene Reihenfolge (Sprint-artiger Working Plan)**

1. **Server Skeleton + Register/Login + Tokens + In-Memory Users** (1–2 Tage)  
2. **Media CRUD (in-memory) + Permission Checks** (1 Tag)  
3. **Postman + README + erster Commit** (0.5–1 Tag)  
4. **PostgreSQL + Repository Layer + Migrations** (1–2 Tage)  
5. **Ratings + Likes + Favorites + Avg Score + Tests** (3–4 Tage)  
6. **Filtering / Search / Sort Endpoints** (1–2 Tage)  
7. **Recommendation + Leaderboard + Tests** (2–3 Tage)  
8. **Doc Polish + Final ZIP** (1 Tag)  
