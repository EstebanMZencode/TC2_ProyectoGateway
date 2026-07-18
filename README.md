# TC2 - Gateway

## Explicación del Repositorio 
El Repositorio contiene la versión final de la tarea en la rama develop actualmente. Consiste en una solución basada en la propuesta del profesor para la Tarea Corta 2 del curso de Programación V, para el segundo cuatrimestre del 2026.

Los elementos de software proporcionados consisten en 4 servicios:
1. MicroservicioCatalogo: CRUD de Productos
2. MicroServicioOrdenCompra
3. MicroServicioCliente
4. ServicioGateway

El ServicioGateway implementa para el mismo Yarp.

## Instalación y Ejecución
Estos Microservicios fueron programados en NET 10 y en lenguaje C#.

En el caso de MicroServicioCliente y ServicioGateway, pueden ser ejecutados sin dificultad en Visual Studio 2026.
El MicroServicioCatalogo puede ejecutarse desde Windows Powershell, ubicándose en la carpeta del servicio (Ej. TC2-Gateway/src/MicroServicioCatalogo). El comando dotnet run permite ejecutar el servicio.

### Requerimientos para la Base de Datos
La base de datos debe ser NoSQL de MongoDB. Deben crearse 3 bases de datos:
  1) Products: que contendrá las colecciones "categories" y "products".
  2) OrdenesDB: que contendrá la colección "orders".
  3) Customers: que contendrá la colección "customers".

Los Microservicios no cuentan en su alcance la colección "categories", por eso deben crearse estas desde la base de datos, y al generar productos, deben crearse con alguna categoría registrada en la colección respecitva.

Esta es la estructura que debe tener un elemento de "categories" (Ejemplo):
{
  "nombre": "Electrónica",
  "descripcion": "Dispositivos electrónicos y accesorios"
}

Esta es la estructura que debe tener un elemento de "products" (Ejemplo):
{
  "sku": "PROD-001",
  "nombre": "Laptop Lenovo - Actualizada",
  "descripcion": "Laptop 17 pulgadas, 16GB RAM, 512GB SSD",
  "categoria": "Electrónica",
  "precio": {
    "$numberDecimal": "400.00"
  },
  "stock": 20,
  "activo": true,
  "fechaCreacion": "2026-01-15T10:00:00Z",
  "fechaActualizacion": {
    "$date": "2026-06-18T21:00:51.222Z"
  }
}

Esta es la estructura que debe tener elemento de "customers" (Ejemplo):
{
  "_id": {
    "$oid": "6a342abac3c2784bcb6f8a32"
  },
  "cedula": "1-2345-6789",
  "nombre": "María Fernández",
  "email": "maria.fernandez@email.com",
  "telefono": "8888-1234",
  "direccion": {
    "provincia": "Cartago",
    "canton": "Cartago",
    "distrito": "Oriental",
    "detalle": "200m norte del parque central"
  },
  "activo": true,
  "fechaRegistro": "2026-03-20T09:15:00Z"
}
