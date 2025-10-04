# 🎓 Portal Académico - Gestión de Cursos y Matrículas
Sistema web para gestionar cursos, estudiantes y matrículas universitarias con autenticación, roles, cache distribuido y sesiones persistentes.


## 📋 Descripción del Proyecto
Portal web interno de una universidad que permite:
- 👨‍🎓 Estudiantes: Consultar catálogo, inscribirse en cursos, gestionar matrículas
- 👨‍💼 Coordinadores: Administrar cursos, aprobar/rechazar matrículas
- 🔐 Sistema de autenticación con **ASP.NET Identity**
- ⚡ Cache distribuido con **Redis** (60s para listado de cursos)
- 💾 Sesiones persistentes (último curso visitado)


👥 Usuarios de Prueba
ROL		      | EMAIL			                  	 | CONTRASEÑA
Coordinador	| coordinador@universidad.edu	   | Coord123!
Estudiante	| (crear desde Register)	       | (crear desde Register)


## 🛠️ Stack Tecnológico
| Componente                 | Tecnología                                    |
|----------------------------|-----------------------------------------------|
| **Framework**              | ASP.NET Core MVC (.NET 8)                     |
| **Autenticación**          | ASP.NET Core Identity                         |
| **Base de Datos**          | SQLite (desarrollo) / PostgreSQL (producción) |
| **ORM**                    | Entity Framework Core                         |
| **Cache/Sesiones**         | Redis (StackExchange.Redis)                   |
| **Frontend**               | Razor Views + Bootstrap 5 + Bootstrap Icons   |
| **Control de Versiones**   | Git + GitHub                                  |


| Rama | Descripción | Estado | Comando |
|------|-------------|--------|---------|
| `main`                       | Rama principal (producción)       | ✅ Protegida | `git checkout main`                      |
| `feature/bootstrap-dominio`  | Pregunta 1: Modelos y DB          | ✅ Merged | `git checkout -b feature/bootstrap-dominio` |
| `feature/catalogo-cursos`    | Pregunta 2: Catálogo con filtros  | ✅ Merged | `git checkout -b feature/catalogo-cursos`   |
| `feature/matriculas`         | Pregunta 3: Inscripciones         | ✅ Merged | `git checkout -b feature/matriculas`        |
| `feature/sesion-redis`       | Pregunta 4: Redis y sesiones      | ✅ Merged | `git checkout -b feature/sesion-redis`      |
| `feature/panel-coordinador`  | Pregunta 5: CRUD coordinador      | ✅ Merged | `git checkout -b feature/panel-coordinador` |
