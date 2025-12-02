# Media Ratings Platform (MRP)  
Pure HTTP REST Server – Intermediate Submission  
Author: Erion Polisi  
GitHub: https://github.com/erionpolisi/MRP  

## 🚀 Overview
The Media Ratings Platform (MRP) is a pure C# HTTP server built **without frameworks**.  
It provides:
- User registration/login (token-based authentication)
- CRUD operations for media entries
- User profiles, favorites, ratings
- In-memory repositories  
- Modular handler-based routing

## 🏗 Architecture
- **HttpRestServer** – minimal custom HTTP server  
- **Handlers** – UserHandler, MediaHandler, VersionHandler  
- **Models** – User, MediaEntry, Rating, Session  
- **Repositories** – in-memory storage  
- **Services** – UserService, (MediaService planned)  
- **Extensions** – helper methods for responses & authentication

## 🔐 Authentication
Token-based:
- 24-char random token
- Session stored in-memory
- 30-minute timeout
- Required for all protected routes

Header:
Authorization: Bearer <token>

## 📡 Implemented Endpoints
### Users
- `POST /users/register`
- `POST /users/login`
- `GET /users/{id}/profile`
- `PUT /users/{id}/profile`
- `GET /users/{id}/ratings`
- `GET /users/{id}/favorites`
- `GET /users/{id}/recommendations`

### Media
- `POST /media`
- `GET /media`
- `GET /media/{id}`
- `PUT /media/{id}`
- `DELETE /media/{id}`
- `POST /media/{id}/rate`
- `GET /media/{id}/ratings`
- `POST /media/{id}/favorite`
- `DELETE /media/{id}/favorite`

## 🧪 Integration Tests
A full **Postman Collection** is included in the repository:  
- Automatically stores token & mediaId  
- Covers all implemented endpoints  

## ▶️ Run the Server
dotnet run
Server starts at: http://localhost:8080


## 📄 Documentation
A detailed **Protokoll.md** (architecture, decisions, UML, requirements) is included in docs.

---

