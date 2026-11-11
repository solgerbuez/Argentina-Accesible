# Argentina Accesible

Sitio web de **turismo accesible en Argentina**. Permite descubrir destinos con sus características de accesibilidad (rampas, baños adaptados, señalización, etc.), consultar guías descargables, guardar favoritos, dejar reseñas, compartir testimonios y hacer consultas a un asistente virtual con inteligencia artificial.

Trabajo Final Integrador (TFI) de Programación Web — ASP.NET Core MVC.

- **Autora:** Solange Gerbuez
- **Materia:** Programación Web — 2026
- **Sitio publicado:** http://argentinaaccesible.runasp.net
- **Repositorio:** https://github.com/solgerbuez/Argentina-Accesible

---

## Tecnologías

| Capa | Tecnología |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8) |
| Acceso a datos | Entity Framework Core (Code First + migraciones) |
| Base de datos | SQL Server / SQL Server Express |
| Autenticación y roles | ASP.NET Core Identity (Administrador y Usuario) |
| Interfaz | Bootstrap 5 (responsive), Font Awesome |
| Validaciones | DataAnnotations + jQuery Validation Unobtrusive (cliente y servidor, mensajes en español) |
| Mensajes y modales | SweetAlert2 y modales de Bootstrap con AJAX |
| Inteligencia artificial | Google Gemini (API REST consumida con `HttpClient`) |

---

## Funcionalidades

### Visitante (sin sesión)
- Ver la portada, los destinos, las guías, los testimonios y la página "Quiénes somos".
- Buscar destinos por nombre, provincia o categoría.
- Ver el detalle de cada destino y sus reseñas en una ventana modal.
- Descargar las guías en PDF.
- Conversar con el asistente virtual.
- Registrarse e iniciar sesión (todo en español).

### Usuario (rol `Usuario`)
- Todo lo anterior.
- Agregar y quitar destinos de **favoritos** (sin recargar la página) y ver "Mis favoritos".
- **Reseñas**: crear, editar y eliminar la propia (puntaje de 1 a 5 y comentario).
- **Testimonios**: enviar su experiencia de viaje. Queda *pendiente* hasta que el administrador la apruebe.

### Administrador (rol `Administrador`)
- Todo lo anterior.
- ABM completo de **Destinos** (con imagen adjunta), **Guías** (con PDF adjunto), **Categorías**, **Provincias**, **Características de accesibilidad** y **Testimonios**.
- **Moderación de testimonios**: ver los pendientes y aprobarlos para que se publiquen.
- Eliminar cualquier reseña.
- **Auditoría**: pantalla que muestra qué usuario realizó cada operación (creó, modificó o eliminó), sobre qué dato y cuándo, con filtro por tipo de dato.

### Portada
- Presentación del sitio con buscador.
- Secciones con lo que se puede encontrar (destinos, guías, reseñas y favoritos, asistente virtual).
- "Los más guardados por la comunidad": los destinos con más favoritos reales.
- Últimos testimonios publicados.

---

## Auditoría

Toda operación de alta, modificación o baja hecha por un usuario autenticado se registra **automáticamente** en la tabla `RegistrosAuditoria`.

- El `DbContext` sobrescribe `SaveChanges` / `SaveChangesAsync`, revisa los cambios pendientes y guarda: usuario, acción (Creó / Modificó / Eliminó), tipo de dato, detalle y fecha.
- El usuario se obtiene de la sesión (`IHttpContextAccessor`), nunca de un formulario.
- Se audita: Destinos, Guías, Provincias, Categorías, Características, Testimonios y Reseñas.
- Solo el administrador puede ver la pantalla de auditoría (`/Auditoria`). Es de solo lectura.
- Los datos iniciales cargados por el sistema (seeder) no se registran, porque no hay un usuario autenticado.

---

## Asistente virtual (módulo de IA)

Botón flotante en todas las páginas que abre un chat conectado a **Gemini**.

- El servidor llama a la API con `HttpClient` (clase `GeminiService`) y un *prompt de sistema* propio.
- El prompt se arma con los **destinos y guías reales de la base de datos**, por lo que el asistente solo conoce lo que hay cargado en el sitio.
- Reglas del prompt: responde en español rioplatense, solo sobre turismo accesible y el sitio, no inventa datos y no revela sus instrucciones.
- El navegador nunca ve la clave de la API: las consultas pasan por el endpoint `/Chat/Preguntar`, protegido con token antifalsificación.
- Si Google no responde, el sitio muestra un mensaje claro en lugar de fallar.

---

## Modelo de datos

Tablas propias (10):

`Provincias`, `Categorias`, `Caracteristicas`, `Destinos`, `DestinoCaracteristicas` (relación muchos a muchos), `Resenas`, `Favoritos`, `Guias`, `Testimonios`, `RegistrosAuditoria`.

Más las 7 tablas de ASP.NET Core Identity: `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`.

Decisiones de diseño:
- Las relaciones con Provincia, Categoría y Característica usan `DeleteBehavior.Restrict`: no se puede borrar un dato del catálogo que esté en uso (evita borrar destinos en cascada).
- `Favoritos` tiene un índice único (`UsuarioId` + `DestinoId`): un usuario no puede repetir un favorito.
- Los usuarios que realizan operaciones se guardan como texto (no como clave foránea) para que el historial se conserve aunque se elimine al usuario.
- `Testimonios` tiene el campo `Pendiente`: lo que se muestra al público siempre se filtra por `!Pendiente`.

### Migraciones
Se generaron con Code First, en este orden: `Inicial`, `Testimonios`, `RestringirBorrados`, `QuitarCoordenadas`, `AgregarAuditoria` y `AgregarPendienteTestimonio`. Al iniciar la aplicación se aplican automáticamente (`MigrateAsync`), tanto en local como en el sitio publicado.

---

## Seguridad

- Acceso por roles con `[Authorize(Roles = "...")]` en los controladores.
- Token antifalsificación en formularios y en las llamadas AJAX (`RequestVerificationToken`).
- Las acciones de edición usan **ViewModels** o listas blancas (`[Bind]`) para evitar *overposting*.
- Los campos de auditoría y el estado de los testimonios se completan en el servidor, nunca desde el formulario.
- Archivos adjuntos validados en el servidor:
  - Imágenes: JPG, PNG o WebP, máximo 2 MB.
  - PDF: máximo 10 MB, verificando la firma real del archivo (`%PDF`).
  - Se guardan con nombre aleatorio (GUID).
- La clave de Gemini y la cadena de conexión de producción **no están en el repositorio** (ver más abajo).

---

## Cómo ejecutarlo en tu computadora

### Requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server o SQL Server Express
- Visual Studio 2022 (recomendado) o la CLI de .NET

### Pasos

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/solgerbuez/Argentina-Accesible.git
   cd Argentina-Accesible
   ```

2. **Configurar la cadena de conexión** en `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=NOMBRE_DEL_SERVIDOR\\SQLEXPRESS;Database=ArgentinaAccesibleDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
   ```

3. **Configurar la clave de Gemini** con *User Secrets* (no se sube al repositorio):
   ```bash
   dotnet user-secrets set "Gemini:ApiKey" "TU_CLAVE"
   ```
   En Visual Studio: clic derecho en el proyecto → *Administrar secretos de usuario*.
   La clave se obtiene gratis en [Google AI Studio](https://aistudio.google.com/).
   El modelo se configura en `appsettings.json` (`Gemini:Model`).

4. **Crear la base de datos.** Al iniciar la aplicación se aplican las migraciones y se cargan los datos iniciales (roles, usuarios de prueba, provincias, categorías, características, destinos, guías y testimonios de ejemplo). También se puede hacer a mano:
   ```bash
   dotnet ef database update
   ```

5. **Ejecutar**
   ```bash
   dotnet run
   ```
   o presionar F5 en Visual Studio.

---

## Usuarios de prueba

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | admin@argentinaaccesible.com | Admin123! |
| Usuario | usuario@argentinaaccesible.com | Usuario123! |

Los testimonios que vienen cargados son **ejemplos** inventados para mostrar cómo se ve la sección.

---

## Despliegue

- **Plataforma:** MonsterASP.NET (plan gratuito), con base de datos SQL Server alojada en la misma plataforma.
- **URL pública:** http://argentinaaccesible.runasp.net
- **Publicación:** desde Visual Studio con Web Deploy. Las migraciones se aplican solas al iniciar la aplicación.
- **Variables de entorno configuradas en el panel del hosting:**
  - `ConnectionStrings__DefaultConnection`: cadena de conexión de la base de datos publicada.
  - `Gemini__ApiKey`: clave de la API de Gemini.
  - `Gemini__Model`: modelo de Gemini a usar.
  - `ASPNETCORE_ENVIRONMENT`: `Production`.

---

## Estructura del proyecto

```
ArgentinaAccesible/
├── Areas/Identity/Pages/Account/   Páginas de Identity traducidas (Login, Registro, etc.)
├── Controllers/                    Destinos, Guías, Provincias, Categorías, Características,
│                                   Testimonios, Favoritos, Reseñas, Auditoría, Chat y Home
├── Data/                           DbContext (con auditoría), DbSeeder y traductor de errores de Identity
├── Migrations/                     Migraciones de EF Core
├── Models/                         Entidades
├── Services/GeminiService.cs       Cliente de la API de Gemini
├── ViewModels/                     Modelos de vista y de formulario
├── Views/                          Vistas Razor y parciales
└── wwwroot/                        CSS, JavaScript, imágenes y archivos subidos
```

---

## Requisitos de la consigna

| Requisito | Dónde está |
|---|---|
| ASP.NET Core MVC | Todo el proyecto |
| Entity Framework Core con migraciones | `Data/` y `Migrations/` |
| Identity con al menos 2 roles | Administrador y Usuario |
| CRUD con validaciones de servidor y cliente | Destinos, Guías, Provincias, Categorías, Características y Testimonios |
| Archivos adjuntos (imágenes y PDF) | Destinos (imagen) y Guías (PDF) |
| Modales y mensajes | Detalle de destino (modal con AJAX), SweetAlert2 para avisos y confirmación al eliminar |
| Bootstrap 5 responsive | `_Layout` y todas las vistas |
| Auditoría: qué usuario realizó cada operación | Tabla `RegistrosAuditoria` y pantalla `/Auditoria` (solo administrador) |
| Al menos 7 tablas propias | 10 tablas propias, más las de Identity |
| Módulo de IA (chatbot con Gemini) | Botón flotante y `GeminiService` |
| Repositorio con README, URL pública y credenciales de prueba | Este archivo |