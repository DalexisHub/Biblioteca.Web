# Biblioteca Viva · MVC con Dapper SEMANA08

Aplicación ASP.NET Core MVC en **.NET 10**, dentro de un único proyecto. Catálogo de libros, socios y reporte de préstamos con Dapper y SQL Server. La interfaz editorial es propia, adaptable a móvil y usa únicamente CSS, fuentes del sistema y scripts locales.

## Ejecutar

```bash
dotnet restore
dotnet build
dotnet run --launch-profile http
```

Abrir http://localhost:5081. Configuración central en `ConnectionStrings:BibliotecaDB`, dentro de `appsettings.json`, leída mediante `IConfiguration`. El marcador `COMPLETAR_SERVIDOR` debe reemplazarse por una conexión válida. También se admite `appsettings.Local.json` (ignorado por Git) y la variable `ConnectionStrings__BibliotecaDB`. Las variables de entorno y argumentos tienen prioridad. No guardar contraseñas en archivos versionados.

## Esquema confirmado

Se inspeccionó `/Users/alexis/Biblioteca-ArquitecturaCapas-Semana07/Database/01_Schema.sql`, `02_Seed.sql`, las entidades, la clase `Conexion` y `Lab07/App.config`.

| Tabla | Columnas |
|---|---|
| Autores | AutorId, Nombre (150), Nacionalidad (100), Activo |
| Libros | LibroId, Titulo (200), ISBN (20, único), AutorId, Ejemplares (>= 0), Activo |
| Socios | SocioId, DNI (15, único), Nombre (150), Email (150), Activo |
| Prestamos | PrestamoId, SocioId, FechaPrestamo, FechaLimite, Estado (Pendiente/Devuelto) |
| DetallePrestamo | PrestamoId + LibroId (PK), FechaDevolucion nullable |

La conexión anterior es `Server=(localdb)\MSSQLLocalDB;Database=BibliotecaDB;Trusted_Connection=True;TrustServerCertificate=True;`. No se encontró otro servidor o credenciales. Los scripts son evidencia del esquema, pero no demuestran que la base instalada conserve exactamente esa estructura.

**No ejecutar el `01_Schema.sql` original sobre una base existente:** elimina tablas. Los archivos de semana 7 se consultaron como referencia y permanecen sin cambios.

## Procedimientos

`Database/08_Procedimientos.sql` contiene diez procedimientos `CREATE OR ALTER`, con prefijo `S08_` para no interferir con otros módulos. Se puede repetir sin duplicarlos. No crea bases, tablas ni datos de prueba. Su ejecución solo instala procedimientos; las operaciones de escritura ocurren cuando se invocan.

- Libros: listar activos con autor, buscar título literal (también `%` y `_`), obtener activo por ID, insertar, actualizar y baja lógica.
- Autores: lista de activos para el desplegable.
- Socios: lista de activos e inserción. El índice único existente protege DNI duplicados incluso con solicitudes concurrentes; el repositorio convierte el error en validación MVC.
- Préstamos: INNER JOIN de las cuatro tablas, una fila por libro, filtrado por **FechaPrestamo** desde el inicio de `desde` hasta el final de `hasta`. Conserva el historial de socios/libros inactivos.

La baja de libros preserva la regla de semana 7: rechaza préstamos pendientes. No existe borrado físico. Los procedimientos y la aplicación validan autores activos. Los ISBN repetidos también muestran validación.

## SQL Server con Docker Compose

```bash
# .env ya se generó localmente con una clave nueva aleatoria, sin publicarla.
# Para otra instalación, copiar .env.example a .env y definir una clave segura.
docker compose up -d
docker compose ps
```

Servidor local: `127.0.0.1,14333`, usuario `sa`, clave en `.env`, edición Developer. El puerto se publica solo en loopback y los datos permanecen en el volumen `biblioteca-viva_biblioteca_sql`. `docker compose stop` detiene el servidor conservando el volumen.

**Inicialización local autorizada y completada el 7 de octubre de 2026.** Se creó `BibliotecaDB` en Docker con el esquema confirmado y los datos de prueba de semana 7: 8 autores, 20 libros, 10 socios, 5 préstamos y 8 detalles. La base y el proyecto originales no se modificaron.

Para una instalación nueva, `bash scripts/inicializar-local.sh` ejecuta `Database/07_InicializarLocal.sql` y los procedimientos. El inicializador rechaza una base ya existente antes de tocar tablas o datos; no contiene `DROP` ni `DELETE`. Compose por sí solo no inicializa datos. Para actualizar la instalación actual usa solamente `bash scripts/aplicar-procedimientos.sh`.

La conexión local ya está en `appsettings.Local.json`. Una copia de la base de Windows podría contener cambios posteriores que no estén en los scripts; esta instalación reproduce los datos de prueba confirmados.

Después de disponer de BibliotecaDB:

```bash
python3 scripts/configurar-local.py
bash scripts/aplicar-procedimientos.sh
dotnet run --launch-profile http
```

El primer comando escribe la conexión en `appsettings.Local.json` sin imprimir la clave ni sobrescribir un archivo existente. Para este contenedor local usa `TrustServerCertificate=True` por el certificado autofirmado; `appsettings.json` mantiene validación TLS habilitada para servidores externos.

En este Mac ARM64 se usa `platform: linux/amd64`. El contenedor se comprobó saludable en este equipo; Microsoft no ofrece soporte para SQL Server bajo emulación. Referencia: https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker

## Organización MVC

- `Models`: modelos y DataAnnotations alineados con los tamaños reales del esquema.
- `Repositorios`: Dapper con `QueryAsync`/`ExecuteAsync`, parámetros y `CommandType.StoredProcedure`. La conexión se crea y dispone aquí.
- `Controllers`: coordinación asíncrona, validación, selección de vistas, PRG y `TempData`. Sin SQL ni conexiones.
- `Views`: Razor fuertemente tipado, `ViewData["Title"]`, fechas conservadas en `ViewData`, parcial de fila y formulario reutilizable.
- `DatosExceptionFilter`: respuesta 503 comprensible cuando la conexión falla, sin exponer detalles de SQL al navegador.

Los POST usan antiforgery y listas explícitas de campos para impedir sobreasignación. Los IDs inexistentes devuelven 404; Edit rechaza IDs inconsistentes.

## Verificación reproducible

```bash
# Pruebas de modelos/controladores dentro del mismo proyecto; repositorios simulados.
dotnet run --no-launch-profile -- --verificar

# Con la aplicación iniciada: pruebas HTTP sin escribir datos.
python3 scripts/verificar-http.py
```

Se comprobaron compilación sin advertencias, 15 pruebas MVC y 12 pruebas HTTP: DataAnnotations, autores válidos, PRG/TempData, baja solo por POST, DNI duplicado traducido a ModelState, IDs inconsistentes/inexistentes, filtros de fechas, antiforgery y recursos locales. Se inspeccionaron vistas en navegador y pantalla móvil de 390 px.

**Integración SQL Server verificada:** se instalaron los diez procedimientos dos veces sin duplicados y se probaron los flujos HTTP con Dapper contra Docker. Pasaron 21 comprobaciones reales: catálogo/autor, búsqueda, autores activos, validación, alta/edición/detalle/baja de libros, ISBN duplicado, protección de préstamos pendientes, socios, DNI duplicado y reportes con fechas inclusivas y estados vacíos.

```bash
# Solo contra la instancia Docker de desarrollo y con la aplicación iniciada.
python3 scripts/verificar-integracion.py --docker-local
```

Esta prueba inserta un libro y un socio identificados con `QA8` y los conserva inactivos al finalizar; no realiza borrados físicos. Compara huellas SHA-256 de todas las filas previas para confirmar que no cambiaron. Tras la primera prueba quedaron 20 libros y 10 socios activos, más un libro y un socio QA inactivos. Las pruebas no requieren otra base ni otro proyecto.

## Referencias visuales y de clase

Se consultó la presentación teórica Semana 8 para separar responsabilidades MVC y usar Razor/ViewData. El enunciado del desafío determina las operaciones y el acceso con Dapper.

Inspiración visual, sin copiar recursos o depender de sus imágenes:
- https://dribbble.com/shots/17344932-Shelfton-Library-Management-Dashboard
- https://dribbble.com/shots/27125194-Libra-Books-Page-Library-Management-Dashboard-Figma
