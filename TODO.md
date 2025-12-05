# ✅ TODOs für die Final Submission (MRP) 
                        ---Specification made with AI---
                
Dieses Dokument listet alle noch offenen Arbeiten für die **Final-Abgabe** der  
**Media Ratings Platform (MRP)** nach offizieller Projekt-Spezifikation und Checkliste auf.

Die Intermediate-Abgabe wurde erfolgreich abgeschlossen – nun folgen die finalen Features,
Business Logic, Datenbank-Anbindung, Tests und vollständige Dokumentation.

---

# 🚀 1. Datenbank (Pflicht)
Die bisherige In-Memory-Logik muss vollständig auf PostgreSQL migriert werden.

## TODOs:
- [ ] PostgreSQL via Docker bereitstellen  
- [ ] Erstellung des vollständigen DB-Schemas  
  - [ ] `users`
  - [ ] `sessions` (optional: in-memory)
  - [ ] `media`
  - [ ] `ratings`
  - [ ] `rating_likes`
  - [ ] `favorites`
- [ ] Repository-Klassen auf DB-Zugriff umstellen (Npgsql)
- [ ] Fehlerhandling: DB down → HTTP 5xx zurückgeben

---

# 🔐 2. Verbesserte Authentifizierung & Access Control
## TODOs:
- [ ] Zugriffskontrollen für alle geschützten Resourcen:
  - [ ] User kann **nur eigene Ratings** ändern/löschen
  - [ ] User kann **nur eigene Media-Einträge** ändern/löschen
- [ ] Token-Handling mit DB-Validierung (oder Memory + Cleanup)

---

# ⭐ 3. Ratings – vollständige Business Logic
## TODOs:
- [ ] `PUT /ratings/{id}` → Rating bearbeiten
- [ ] `DELETE /ratings/{id}` → Rating löschen
- [ ] `POST /ratings/{id}/like` → Like-System:
  - [ ] Nur 1 Like pro User
  - [ ] Like-Zähler aktualisieren
- [ ] Kommentar-Moderation:
  - [ ] Kommentar erst nach `POST /ratings/{id}/confirm` sichtbar
  - [ ] Ratings ohne bestätigten Kommentar → Kommentar im Public-Response ausblenden
- [ ] Reihenfolge der Ratings nach Datum sortieren (timestamp)

---

# 🎬 4. Media – erweiterte Funktionen
## TODOs:
- [ ] Media-Filter implementieren:
  - [ ] `?genre=...`
  - [ ] `?type=movie|series|game`
  - [ ] `?year=2023`
  - [ ] `?age=16`
  - [ ] `?minScore=4`
- [ ] Sortieren:
  - [ ] `?sort=title`
  - [ ] `?sort=year`
  - [ ] `?sort=score`
- [ ] Zugriffskonrolle für Update/Delete → nur Creator!

---

# 🔥 5. Recommendations – vollständig implementieren
## TODOs:
### A) Genre-Based
- [ ] Finde Genres, die der User am besten bewertet → empfehle ähnliche Media

### B) Content-Based
- [ ] Ähnlichkeit basierend auf:
  - [ ] Genre-Overlap
  - [ ] MediaType
  - [ ] AgeRestriction
- [ ] Bewertungshistorie berücksichtigen

---

# 🏅 6. Leaderboard
## TODOs:
- [ ] öffentlich sichtbare Liste der aktivsten User
- [ ] Sortierung nach:
  - [ ] Anzahl der Ratings
  - Optional:
    - [ ] Anzahl Lieblingsmedien
    - [ ] Durchschnittliche Bewertung
- [ ] `GET /leaderboard` korrekt implementieren

---

# 👤 7. User-Profil – vollständige Statistiken
## TODOs:
- [ ] Gesamtzahl aller Bewertungen (`totalRatings`)
- [ ] Durchschnittliche Bewertung (`averageScore`)
- [ ] Lieblingsgenre (`favoriteGenre`)
- [ ] Anzahl der Favoriten (`favoritesCount`)
- [ ] Kürzliche Aktivität (optional)
- [ ] Profil-Response erweitern

---

# 🧪 8. Unit Tests (mindestens 20)
Pflicht laut Spezifikation.

## TODOs:
- [ ] Rating-Logik testen (Stars, Validation, Updates)
- [ ] Like-System testen
- [ ] Favorites testen
- [ ] Recommendation Engine testen
- [ ] Filter & Sortierlogik testen
- [ ] Ownership-Checks testen
- [ ] DB-Repositorys testen (optional Stub/Mock)

---

# 🧰 9. Integration Tests (Postman)
## TODOs:
- [ ] Final Postman Collection aktualisieren
- [ ] Alle Endpoints abdecken:
  - Registrierung
  - Login
  - Media CRUD
  - Favorites
  - Ratings + Edit/Delete/Like/Confirm
  - Filtering
  - Leaderboard
  - Recommendations

---

# 📄 10. Dokumentation (Protokoll)
## TODOs:
- [ ] Architekturentscheidungen beschreiben
- [ ] Datenbankmodell dokumentieren
- [ ] UML-Diagramm aktualisieren (inkl. DB + Services)
- [ ] Teststrategie erklären
- [ ] Zeitaufwand pro Teil
- [ ] Lessons learned + Probleme & Lösungen
- [ ] Screenshots (optional) der Tests einfügen

---

# 🎉 Abschluss
Wenn alle Punkte erfüllt sind → entspricht deine Anwendung **vollständig**  
den FINAL REQUIREMENTS laut Spezifikation und offizieller Bewertungs-Checkliste.

