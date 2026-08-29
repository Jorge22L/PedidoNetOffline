# PedidoNet

Sistema de facturación, inventario y gestión de pedidos desarrollado con **ASP.NET Core** y **Blazor WebAssembly PWA**.

El proyecto está dividido en un backend basado en API REST y un cliente web independiente construido con Blazor WebAssembly.

La aplicación se desarrolla de forma incremental. En su primera etapa, el cliente permitirá consumir la API, autenticarse, consultar información y trabajar con formularios. Posteriormente se incorporarán capacidades offline utilizando IndexedDB y mecanismos de sincronización.

---

## Objetivo

El objetivo del proyecto es construir una aplicación web moderna que permita gestionar operaciones relacionadas con:

- Productos
- Clientes
- Pedidos
- Facturación
- Inventario
- Ventas
- Caja
- Autenticación y autorización
- Sincronización de datos

La aplicación cliente estará preparada como **Progressive Web App (PWA)** para poder instalarse y, en etapas posteriores, continuar funcionando durante interrupciones de conectividad.

---

## Estado actual

El proyecto se encuentra en una fase inicial de desarrollo del cliente Blazor WebAssembly.

Actualmente el trabajo está enfocado en:

- Crear el cliente Blazor WebAssembly
- Configurarlo como PWA
- Integrar Bootstrap
- Consumir la API existente
- Implementar autenticación mediante JWT
- Crear componentes reutilizables
- Crear formularios con validación
- Implementar inicialmente el módulo de Productos

Las funcionalidades offline, IndexedDB y sincronización se implementarán posteriormente.

---

# Tecnologías

## Backend

- .NET
- ASP.NET Core Web API
- Entity Framework Core
- JWT Authentication
- Authorization Policies
- Dependency Injection
- Swagger / OpenAPI

## Frontend

- .NET
- Blazor WebAssembly
- Progressive Web App
- Razor Components
- HTML
- CSS
- Bootstrap
- JavaScript Interop cuando sea necesario

## Persistencia

### Servidor

La persistencia definitiva de los datos es responsabilidad del backend.

### Cliente

En una etapa posterior se utilizará:

- IndexedDB
- Repositorios locales
- Cola de sincronización

---

# Arquitectura general

```text
┌──────────────────────────────────────┐
│       Blazor WebAssembly PWA         │
│                                      │
│  Pages                               │
│  Components                          │
│  Services                            │
│  Auth                                │
│                                      │
│  IndexedDB        ← futuro           │
│  SyncService      ← futuro           │
└───────────────────┬──────────────────┘
                    │
                    │ HTTPS / JSON
                    │ JWT Bearer Token
                    ▼
┌──────────────────────────────────────┐
│          ASP.NET Core API            │
│                                      │
│  Controllers                         │
│       │                              │
│       ▼                              │
│  Application                         │
│       │                              │
│       ▼                              │
│  Domain                              │
│       ▲                              │
│       │                              │
│  Infrastructure / Persistence        │
└───────────────────┬──────────────────┘
                    │
                    ▼
               Base de Datos
```

El cliente nunca accede directamente a la base de datos.

Toda operación remota debe pasar por la API.

---

# Estructura de la solución

```text
PedidoNet/
│
├── src/
│   │
│   ├── Api/
│   ├── Application/
│   ├── Domain/
│   ├── Infrastructure/
│   ├── Middleware/
│   ├── Persistence/
│   └── Client/
│
├── ARCHITECTURE.md
├── README.md
├── .gitignore
└── CRUD_Pedidos.sln
```

---

# Backend

## Api

Es el punto de entrada HTTP de la aplicación.

Contiene:

- Controllers
- Configuración de autenticación JWT
- Autorización
- Swagger
- Dependency Injection
- Configuración del pipeline HTTP

Flujo básico:

```text
HTTP Request
     │
     ▼
Controller
     │
     ▼
Application
```

---

## Application

Contiene los casos de uso de la aplicación.

Puede incluir:

- Commands
- Queries
- DTOs
- Validators
- Interfaces
- Mapping
- Handlers

Esta capa coordina las operaciones necesarias para ejecutar las acciones solicitadas por los usuarios.

---

## Domain

Representa el núcleo del negocio.

Contiene conceptos como:

- Producto
- Cliente
- Pedido
- DetallePedido
- Roles
- Constantes
- Contratos de repositorio

Esta capa debe mantenerse independiente de tecnologías de interfaz o infraestructura.

---

## Infrastructure

Contiene implementaciones técnicas de contratos definidos por otras capas.

Ejemplo:

```text
Application
     │
     ▼
IProductRepository
     ▲
     │
Infrastructure
```

---

## Persistence

Responsable del acceso a datos.

Incluye elementos como:

- DbContext
- Entity Framework Core
- Configuraciones de entidades
- Migraciones
- Unit of Work
- Transacciones

Flujo simplificado:

```text
Controller
    │
    ▼
Application
    │
    ▼
Repository
    │
    ▼
DbContext
    │
    ▼
Database
```

---

# Cliente Blazor WebAssembly

El cliente se encuentra en:

```text
src/Client
```

Se ejecuta directamente en el navegador utilizando WebAssembly.

La arquitectura inicial será:

```text
Page
 │
 ▼
Service
 │
 ▼
HttpClient
 │
 ▼
API
```

A medida que el proyecto crezca podrá evolucionar hacia:

```text
Page
 │
 ▼
Service
 │
 ▼
ApiClient
 │
 ▼
HttpClient
 │
 ▼
API
```

---

# Estructura prevista del cliente

```text
Client/
│
├── Components/
│   ├── Common/
│   └── Products/
│
├── Layout/
│
├── Models/
│   ├── Auth/
│   └── Products/
│
├── Pages/
│   ├── Auth/
│   ├── Dashboard/
│   └── Products/
│
├── Services/
│   ├── Api/
│   ├── Auth/
│   └── Products/
│
├── wwwroot/
│
├── App.razor
└── Program.cs
```

La estructura se irá creando progresivamente.

No es necesario implementar todas las carpetas desde el inicio.

---

# Primer módulo: Productos

El primer módulo utilizado para desarrollar el cliente será **Productos**.

Este módulo permite practicar:

- Consumo de API REST
- HttpClient
- Componentes Razor
- Formularios
- Validación
- Bootstrap
- Autenticación
- Autorización
- CRUD

Flujo de consulta:

```text
Usuario
   │
   ▼
Products/Index.razor
   │
   ▼
ProductService
   │
   ▼
HttpClient
   │
   ▼
GET /api/Producto
   │
   ▼
ProductoController
   │
   ▼
Application
   │
   ▼
Repository
   │
   ▼
Database
```

---

# Endpoints principales

## Autenticación

```http
POST /api/Auth/login
POST /api/Auth/refresh
POST /api/Auth/revoke
```

## Productos

```http
GET    /api/Producto
GET    /api/Producto/{id}
POST   /api/Producto
PUT    /api/Producto/{id}
DELETE /api/Producto/{id}
```

## Clientes

```http
GET    /api/Clientes
GET    /api/Clientes/{id}
POST   /api/Clientes
PUT    /api/Clientes/{id}
DELETE /api/Clientes/{id}
```

## Pedidos

```http
GET    /api/Pedidos
GET    /api/Pedidos/{id}
GET    /api/Pedidos/cliente/{clienteId}
POST   /api/Pedidos
PUT    /api/Pedidos/{id}
DELETE /api/Pedidos/{id}
PATCH  /api/Pedidos/{id}/completar
PATCH  /api/Pedidos/{id}/cancelar
GET    /api/Pedidos/estadisticas
```

---

# Autenticación

La API utiliza autenticación mediante JWT.

Flujo:

```text
Usuario
   │
   ▼
Login.razor
   │
   ▼
AuthService
   │
   ▼
POST /api/Auth/login
   │
   ▼
API
   │
   ▼
Access Token
Refresh Token
```

Las peticiones protegidas utilizarán:

```http
Authorization: Bearer <access_token>
```

Posteriormente el cliente manejará:

- Estado de autenticación
- Expiración del token
- Refresh Token
- Logout
- Roles
- Authorization Policies
- `AuthorizeView`
- Rutas protegidas

---

# Formularios

Los formularios se implementarán utilizando componentes de Blazor.

Flujo conceptual:

```text
ProductForm
     │
     ▼
EditForm
     │
     ▼
DataAnnotationsValidator
     │
     ▼
ValidationMessage
     │
     ▼
ProductService
     │
     ▼
API
```

El objetivo es mantener separadas:

- Presentación
- Validación
- Comunicación HTTP
- Lógica de aplicación

---

# Bootstrap

Bootstrap será utilizado como base visual del proyecto.

Se utilizará para construir:

- Navegación
- Formularios
- Tablas
- Botones
- Cards
- Badges
- Alerts
- Modals
- Spinners
- Layout responsive

También se utilizará CSS personalizado para evitar una apariencia genérica.

---

# Ejecución del proyecto

## Requisitos

Se recomienda tener instalado:

- .NET SDK compatible con la solución
- Git
- Visual Studio, Visual Studio Code o JetBrains Rider
- Navegador moderno

Verificar .NET:

```bash
dotnet --version
```

---

# Clonar el repositorio

```bash
git clone <url-del-repositorio>
cd PedidoNet
```

---

# Restaurar dependencias

```bash
dotnet restore
```

---

# Compilar la solución

```bash
dotnet build
```

---

# Ejecutar la API

Desde la raíz del repositorio:

```bash
dotnet run --project src/Api/Api.csproj
```

La URL concreta dependerá de:

```text
src/Api/Properties/launchSettings.json
```

---

# Ejecutar Blazor WebAssembly

Desde la raíz:

```bash
dotnet run --project src/Client/Client.csproj
```

La terminal mostrará la dirección del cliente.

Ejemplo:

```text
https://localhost:7001
```

---

# Ejecutar API y cliente al mismo tiempo

## Terminal 1

```bash
dotnet run --project src/Api/Api.csproj
```

## Terminal 2

```bash
dotnet run --project src/Client/Client.csproj
```

Flujo:

```text
Browser
   │
   ▼
Blazor WebAssembly
   │
   ▼
ASP.NET Core API
   │
   ▼
Database
```

---

# CORS

Durante desarrollo el cliente y la API normalmente se ejecutan en puertos diferentes.

Ejemplo:

```text
Client
https://localhost:7001

API
https://localhost:7002
```

Para el navegador estos representan orígenes diferentes.

La API deberá configurar CORS para permitir explícitamente el origen del cliente.

No se recomienda utilizar `AllowAnyOrigin()` como configuración permanente de producción.

---

# PWA

El proyecto Blazor está preparado para funcionar como Progressive Web App.

Esto permitirá posteriormente:

- Instalar la aplicación
- Ejecutarla como aplicación independiente
- Cachear recursos estáticos
- Trabajar con conectividad limitada
- Mantener datos localmente
- Sincronizar cambios con la API

El Service Worker será responsable principalmente de los recursos estáticos.

Los datos empresariales se almacenarán posteriormente en IndexedDB.

---

# Arquitectura offline futura

```text
                    ┌────► API Client ───► API
                    │
UI ───► Services ───┤
                    │
                    └────► Local Repository ───► IndexedDB
```

Cuando exista conexión:

```text
Usuario
   │
   ▼
Blazor
   │
   ▼
Service
   │
   ▼
API
```

Cuando no exista conexión:

```text
Usuario
   │
   ▼
Blazor
   │
   ▼
Service
   │
   ▼
IndexedDB
   │
   ▼
SyncQueue
```

Cuando regrese Internet:

```text
Internet disponible
        │
        ▼
NetworkService
        │
        ▼
SyncService
        │
        ▼
SyncQueue
        │
        ▼
API
        │
        ▼
IndexedDB actualizado
```

---

# Seguridad

El cliente nunca debe ser considerado una fuente confiable.

La API siempre deberá volver a validar:

- Usuario
- Rol
- Permisos
- Precios
- Descuentos
- Impuestos
- Inventario
- Estado de documentos

También deberán considerarse:

- Expiración de sesión
- Protección de tokens
- Eliminación de información local cuando corresponda
- Validaciones nuevamente en backend
- Autorización por roles y políticas

---

# Flujo completo de código

```text
Usuario
   │
   ▼
Blazor Page
   │
   ▼
Component
   │
   ▼
Service
   │
   ▼
ApiClient / HttpClient
   │
   ▼
HTTP Request
   │
   ▼
Controller
   │
   ▼
Application
   │
   ▼
Domain
   │
   ▼
Repository
   │
   ▼
Persistence
   │
   ▼
Database
```

Respuesta:

```text
Database
   │
   ▼
Persistence
   │
   ▼
Repository
   │
   ▼
Application
   │
   ▼
Controller
   │
   ▼
JSON
   │
   ▼
HttpClient
   │
   ▼
Service
   │
   ▼
Component
   │
   ▼
Usuario
```

---

# Roadmap

## Fase 1 — Cliente base

- [ ] Crear Blazor WebAssembly PWA
- [ ] Configurar Bootstrap
- [ ] Crear layout
- [ ] Configurar HttpClient
- [ ] Configurar CORS
- [ ] Consumir API

## Fase 2 — Productos

- [ ] Listar productos
- [ ] Crear componentes
- [ ] Crear formulario
- [ ] Validar formulario
- [ ] Crear producto
- [ ] Editar producto
- [ ] Eliminar producto

## Fase 3 — Autenticación

- [ ] Crear pantalla Login
- [ ] Consumir `/api/Auth/login`
- [ ] Almacenar JWT
- [ ] Crear `AuthenticationStateProvider`
- [ ] Agregar Bearer Token a peticiones
- [ ] Implementar logout
- [ ] Implementar Refresh Token
- [ ] Trabajar con roles y autorización

## Fase 4 — Persistencia local

- [ ] IndexedDB
- [ ] Repositorios locales
- [ ] Productos locales
- [ ] Clientes locales
- [ ] NetworkService

## Fase 5 — Offline-first

- [ ] SyncQueue
- [ ] SyncService
- [ ] Reintentos
- [ ] Estados de sincronización
- [ ] Sincronización de productos
- [ ] Sincronización de clientes
- [ ] Resolución de conflictos

## Fase 6 — Módulos avanzados

- [ ] Facturación
- [ ] Inventario
- [ ] Ventas
- [ ] Caja
- [ ] Facturación offline
- [ ] Inventario offline

---

# Documentación

Para una explicación más detallada sobre la comunicación entre capas y el flujo interno del código, consultar:

```text
ARCHITECTURE.md
```

---

# Principios del proyecto

El proyecto busca seguir estos principios:

- Separación de responsabilidades
- Bajo acoplamiento
- API como autoridad final
- Componentes reutilizables
- Servicios para lógica del cliente
- Validación en frontend y backend
- Seguridad aplicada siempre en servidor
- Desarrollo incremental
- Offline-first en etapas posteriores
- Sincronización resiliente

---

# Licencia

Proyecto desarrollado con fines educativos para el aprendizaje de desarrollo web utilizando ASP.NET Core y Blazor WebAssembly.