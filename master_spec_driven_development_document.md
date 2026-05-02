Master Specification: GestionPGB (Stock Control System)

Eres un Desarrollador de software experto en C# y .NET, además de ser un experto en arquitectura de software y bases de datos. Tu tarea es analizar y entender esta especificación para comenzar a desarrollar el proyecto.

El proyecto y carpetas ya estan creadas, la base de datos tambien. Tu tarea es comenzar a desarrollar el proyecto desde cero, creando los archivos y carpetas necesarios para el correcto funcionamiento del mismo, implementando todo lo especificado en esta master spec.

############################

1.1 Visión del Producto
Sistema de gestión de inventario local para taller mecánico enfocado exclusivamente en el control físico de stock. El sistema actúa como un monitor y registro de movimientos, eliminando toda complejidad financiera o de precios para priorizar la agilidad operativa en el depósito.

##############

1.2 Stack Tecnológico Definido
Backend: .NET 8 (C#) con Minimal APIs.


Frontend: React / Next.js. (No decidi aún que framework de React usar, me da igual)

Base de Datos: PostgreSQL. 
    -Nombre de la base de datos: GestionPGB-DATABASE
    -Usuario: postgre
    -Contraseña: [PASSWORD] 

Comunicación Real-Time: SignalR.

Entorno de Desarrollo: Claude Code + Spec-Driven Development + Windows 11 + VS .

#############
1.3 Reglas de Negocio Inviolables (Core)
Cero Precios: No se almacena, procesa ni calcula información monetaria en ninguna parte del sistema de stock.

Punto de Pedido: La inteligencia del sistema reside en comparar el current_stock contra el min_required_stock para alertar faltantes.

Pedido de Cotización: En lugar de Órdenes de Compra con valores, se generan "Pedidos de Cotización" que contienen la lista de materiales faltantes y tambien en caso de que el usuario quiera agregar cualquier producto, a pesar que este en stock OK, el sistema deberia .

############################
2.1 Arquitectura de Software
Se utilizará una arquitectura de Monolito Modular organizada por carpetas para facilitar el contexto de la IA:

Domain: Entidades (Product, StockMovement) e interfaces de repositorio.

Application: DTOs, servicios de generación de PDF y lógica de negocio.

Infrastructure: Implementación de AppDbContext (EF Core) y Repositorios.

API: Controladores y Hubs de SignalR.

############################
2.2 Modelo de Datos (PostgreSQL Schema)
Tabla products:

id: UUID (PK).

barcode: VARCHAR (Unique Index) - Código para el lector.

item_name: VARCHAR - Nombre del ítem según fabricante.

description: TEXT - Detalle del producto.

current_stock: INTEGER - Cantidad en estante.

min_required_stock: INTEGER - Umbral de compra.

provider_name: VARCHAR - Para agrupar pedidos.

Tabla stock_movements:

id: UUID.

product_id: FK.

quantity: INTEGER (+1 para entradas, -1 para salidas por defecto).

type: ENUM ('ENTRADA', 'SALIDA', 'AJUSTE').

############################
2.3 Flujo del "Control Remoto" (Sincronización)
Interfaz Móvil: El usuario selecciona el modo de operación (ej: "Ingreso de Mercadería").

Terminal PC: Recibe el input del lector de barras físico.

Sincronización: SignalR empuja el barcode al móvil en tiempo real.

Respuesta: El backend procesa el movimiento y actualiza el stock, notificando a ambas interfaces.

################################
2.4 Salidas del Sistema
PDF de Cotización: Documento con 3 columnas (Ítem, Descripción, Cantidad) agrupado por proveedor.

Directivas para Claude Code (Eficiencia de Tokens)
Ignorar: No analices carpetas bin, obj, ni migrations (ver .claudignore).

Simplicidad: No asumas que el sistema necesita precios en el futuro; mantené el código limpio de lógica financiera.

Naming: Usar snake_case para la base de datos y PascalCase para el código C#.

################################

3.1 Esquema de Identidad
Mecanismo: Autenticación basada en JWT (JSON Web Tokens).

Librería: ASP.NET Core Identity utilizando el proveedor de Entity Framework.

Almacenamiento de Claves: Uso de User Secrets en desarrollo y variables de entorno en producción para el JWT_SECRET.

3.2 Roles de Usuario
Admin: Acceso total (Configuración de productos, gestión de usuarios, generación de pedidos de cotización).

Operator: Acceso limitado solo al escaneo de entradas/salidas y visualización de stock actual.

3.3 Requerimientos de Seguridad
Cifrado de Passwords: Utilizar PasswordHasher nativo de ASP.NET Identity (BCrypt/PBKDF2).

CORS: Configuración estricta para permitir solo el origen del frontend (React).

Protección de Endpoints: Todos los controladores de stock requerirán el atributo [Authorize].


### Flujo de Autenticación:
El usuario se loguea desde el celular o la PC.

El servidor valida credenciales en PostgreSQL y devuelve un token.

El frontend adjunta el token en el header Authorization: Bearer <token> para cada movimiento de stock.

################################

4.1 Documentación de API
Herramienta: Scalar (reemplazando Swagger UI) para una mejor experiencia de prueba y documentación técnica.

Ruta: /scalar/v1.




################################
5.1 Manejo de Estado
Sustento de Sesión: Persistencia del JWT en localStorage o HttpOnly Cookies.

Comunicación Real-Time: Configuración del cliente @microsoft/signalr para escuchar el Hub de la PC.

################################
6. PROTOCOLO DE PRODUCCIÓN: SEGURIDAD, AUDITORÍA Y DEPLOY
Esta sección define los requisitos críticos para la estabilidad y protección del sistema en el entorno real del taller.

6.1 Seguridad y Control de Acceso
Identidad: Autenticación mediante JWT (JSON Web Tokens) gestionada por ASP.NET Core Identity.

Roles: Diferenciación estricta entre Admin (gestión completa) y Operator (solo escaneo y consulta de stock).

Rate Limiting: Protección nativa contra abusos: máximo 5 intentos de login/min y 100 peticiones generales/min por IP.

CORS: Configuración de Whitelist estricta que solo permita el dominio de producción del frontend.

6.2 Auditoría y Logging (Trazabilidad)
Historial de Stock: Registro obligatorio en stock_movements de: user_id (quién), product_id (qué) y timestamp (cuándo).

Logging Técnico: Implementación de Serilog para capturar errores de sistema y guardarlos en la tabla system_logs de PostgreSQL.

Monitoreo Real-Time: Registro de eventos de conexión y desconexión en el Hub de SignalR para auditar la estabilidad de los dispositivos móviles.

Registro de movimientos críticos (quién sacó qué producto y cuándo) en la tabla stock_movements para auditoría básica.

6.3 Configuración de Deployment
Gestión de Secretos: Prohibido el uso de claves hardcodeadas. El sistema debe leer CONNECTION_STRING y JWT_SECRET exclusivamente de variables de entorno en producción.

Optimización: Desactivar SensitiveDataLogging y forzar el uso de .AsNoTracking() en consultas de solo lectura para maximizar performance.

Infraestructura:

Backend + DB: Railway (configurado para despliegue continuo desde el repo).
