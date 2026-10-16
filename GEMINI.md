# LinkUp Pro - Reglas de Proyecto y Contexto para Antigravity CLI (`agy`)

## 1. Identidad y Enfoque Pedagógico
- **Rol del Asistente**: Actúas como un mentor y arquitecto sénior en .NET 9 para el estudiante.
- **Regla de Oro**: **NO escribas todo el proyecto ni entregues bloques masivos de código sin explicación**. El estudiante tiene la intención de aprender C# y ASP.NET Core MVC. Tu función es guiar paso a paso, explicar el porqué de cada decisión arquitectónica y ayudarlo a implementar y depurar cada módulo.
- **Mensajes de Git**: Los mensajes de commit deben ser **cortos, concisos y humanos** (ejemplo: `feat: agregar entidad ApplicationUser y configuracion Fluent API`). Jamás uses comentarios largos o genéricos de IA.

---

## 2. Información del Proyecto y Repositorio
- **Nombre**: LinkUp Pro (Red Social con minijuego Battleship)
- **Repositorio**: `https://github.com/eduardtejada/LinkUpPro.git`
- **Tecnologías**:
  - ASP.NET Core MVC con **.NET 9**
  - Entity Framework Core Code First (con Migraciones)
  - ASP.NET Core Identity (Usuarios, contraseñas seguras, lockout, tokens, sesiones)
  - AutoMapper (Mapeo entre Entidades, DTOs y ViewModels)
  - Bootstrap 5 (UI responsiva y limpia)

---

## 3. Arquitectura Onion (Cebolla) y Estructura de Proyectos
La solución `LinkUpPro.sln` se compone estrictamente de 5 proyectos con responsabilidades desacopladas:

1. **`Core/LinkUpPro.Core.Domain`** (Biblioteca de clases):
   - Entidades del dominio y Enums. Cero dependencias externas o de base de datos.
2. **`Core/LinkUpPro.Core.Application`** (Biblioteca de clases):
   - Interfaces de servicios y repositorios, DTOs, ViewModels de negocio, validaciones y perfiles de AutoMapper. Depende únicamente de `Domain`.
3. **`Infrastructure/LinkUpPro.Infrastructure.Persistence`** (Biblioteca de clases):
   - `ApplicationDbContext`, configuraciones Fluent API, migraciones de EF Core, implementaciones de `GenericRepository<T>` y repositorios específicos. Depende de `Application` y `Domain`.
4. **`Infrastructure/LinkUpPro.Infrastructure.Shared`** (Biblioteca de clases):
   - Servicios compartidos externos: Envío de correos (`IEmailService` con MailKit/SMTP) y almacenamiento local de archivos/imágenes (`IFileStorageService`). Depende de `Application`.
5. **`Presentation/LinkUpPro.Presentation.WebApp`** (ASP.NET Core MVC .NET 9):
   - Controladores, Vistas Razor, ViewModels de presentación, filtros, middlewares y configuración en `Program.cs`.
   - **Prohibido**: No debe existir lógica de negocio compleja ni consultas directas a `DbContext` en los controladores. Las vistas nunca consumen entidades directas de la base de datos (siempre ViewModels).

---

## 4. Requerimientos Funcionales y Rúbrica Crítica (1,510 Puntos)

### A. Autenticación, Registro y Perfil
- **Validación República Dominicana**: Formato de teléfono dominicano requerido (`809-xxx-xxxx`, `829-xxx-xxxx`, `849-xxx-xxxx`).
- **Nombres de usuario y correo únicos**: Insensibles a mayúsculas/minúsculas.
- **Foto de perfil**: Validación de formato (.jpg, .jpeg, .png, .webp) y tamaño $\le$ 5 MB.
- **Indicador de fortaleza de contraseña**: Débil ($\le 2$ criterios), Media (3-4 criterios), Fuerte (5 criterios: 8 caracteres, mayúscula, minúscula, número, carácter especial).
- **Activación de cuenta por correo**: Cuenta inicia inactiva. Token Identity válido por 24 horas y de un solo uso. Reenvío con espera mínima de 5 minutos. Mensaje genérico para no revelar si la cuenta existe.
- **Bloqueo por fuerza bruta**: 5 intentos fallidos bloquean la cuenta por 15 minutos.
- **Sesión**:
  - Si marca "Mantener sesión iniciada": Cookie persistente por 7 días.
  - Si no lo marca: Cierre al cerrar navegador o tras 30 minutos de inactividad.
- **Restablecimiento de contraseña**: Token válido por 1 hora. Restablecer contraseña desbloquea la cuenta e invalida sesiones anteriores, pero NO activa una cuenta inactiva.
- **Mi Perfil**: Edición de nombre, apellido, teléfono y foto opcional. Cambio opcional de clave (valida clave actual e invalida sesiones previas). Correo y nombre de usuario en solo lectura.

### B. Publicaciones (Home) e Interacciones
- **Feed del Home**: Publicaciones propias ordenadas de más reciente a más antigua.
- **Buscador y Filtros combinables**: Búsqueda por contenido, tipo (Imagen o YouTube), rango de fechas y estado de edición (Todas, Editadas, No editadas).
- **Contenido condicional**: Texto + exactamente 1 contenido multimedia (o Imagen o Enlace YouTube incrustado).
- **Privacidad**: `Solo amigos` o `Solo yo`. La privacidad debe validarse en servidor.
- **Comentarios y Respuestas anidadas**:
  - Hilos anidados con indentación.
  - Opción de desactivar comentarios por post (las conversaciones previas permanecen visibles).
  - Borrado lógico: si un comentario tiene respuestas, se sustituye su contenido por `"Este comentario fue eliminado"`.
- **Reacciones**: "Me gusta" y "No me gusta" (una sola activa a la vez por usuario, con opción de eliminar reacción sin notificar).

### C. Amigos y Solicitudes
- **Solicitudes**: Estados `En espera`, `Aceptada`, `Rechazada`, `Cancelada`.
  - Prevención estricta de solicitudes duplicadas en ambas direcciones y auto-solicitudes.
  - Ocultar solicitudes del historial sin eliminarlas físicamente.
- **Amistades**: Estados `Activa` y `Eliminada`.
  - Una única relación para cada pareja de usuarios. Al aceptar solicitud previa eliminada, se **reactiva** la existente.
  - Eliminación lógica y bidireccional. Conserva interacciones históricas y partidas activas de Battleship.
- **Amigos en común**: Algoritmo que calcula amigos activos compartidos (excluyendo a la propia pareja). Lista interactiva.
- **Feed de Amigos**: Pantalla `Amigos` con contador de amigos activos y publicaciones disponibles, más visualización de publicaciones `Solo amigos` de amigos activos.

### D. Notificaciones
- Generadas en comentarios, respuestas y reacciones (excepto si el autor es el mismo usuario).
- Estados Leída / No leída. Botón "Marcar todas como leídas".
- Badges dinámicos en el Navbar para Solicitudes pendientes y Notificaciones no leídas (actualizados al recargar/navegar).

### E. Minijuego Battleship (Batalla Naval)
- Tablero de $12 \times 12$.
- 5 barcos por jugador: tamaños 2, 3, 3, 4 y 5.
- **Fase 1 (Posicionamiento)**:
  - Seleccionar barco $\rightarrow$ celda inicio $\rightarrow$ dirección (arriba, abajo, izquierda, derecha).
  - Validaciones: no salirse del tablero y no superponerse sobre celdas ocupadas.
  - Pantalla de espera hasta que el oponente configure su flota.
- **Fase 2 (Ataque y Turnos)**:
  - Inicia atacando el creador de la partida.
  - Tablero de ataque: Rojo (acierto), Verde (fallo). No permite repetir celdas atacadas.
  - Alternancia estricta de turnos en el backend. Botón manual "Refrescar pantalla".
  - Rendición con confirmación (otorga victoria al rival).
  - **Abandono por inactividad**: Si transcurren 48 horas sin ataque del jugador en turno, la partida finaliza por abandono y gana el rival.
- **Historial y Tableros Finales**:
  - Consulta en solo lectura de tableros de ataque de ambos jugadores y colocación de barcos del usuario.

### F. Seguridad Funcional y Técnica
- `[Authorize]` en todas las rutas internas y `[AllowAnonymous]` en públicas.
- `[ValidateAntiForgeryToken]` en todos los métodos POST/modificaciones.
- Validación de propiedad y privacidad en el servidor para CADA recurso (no confiar en URLs ni campos ocultos).
- Codificación Anti-XSS para todo texto ingresado por el usuario.
- Control de concurrencia y transacciones atómicas en cambios críticos (amistades, ataques, etc.).

---

## 5. Estado Actual del Proyecto y Próximos Pasos
- **Estado**: Solución y 5 proyectos creados y compilando en .NET 9. Repositorio en GitHub sincronizado en rama `main`.
- **Paso Inmediato**: 
  1. Instalar paquetes NuGet requeridos por capa.
  2. Crear entidades de dominio (`ApplicationUser`, `Friendship`, `FriendRequest`, `Post`, `Comment`, `Reaction`, `Notification`, `BattleshipGame`, etc.).
  3. Crear `ApplicationDbContext` y repositorios en Persistence.
