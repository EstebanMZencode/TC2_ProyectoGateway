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
