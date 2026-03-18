El docker-compose.yml levanta 3 servicios:
 - my-dotnet-api
 - my-rest-api
 - traffic-generator

# my-dotnet-api
Es un API con .NET 8 que incluye:
 - un servicio de healthcheck
 - un controlador para añadir y consultar jugadores con parámetros básicos `id` y `name`
 - un SqlLite para persistir los datos, aunque no se están almacenando en volumen y desaparecen al limpiar el contenedor
 - un middleware que registra las peticiones recibidas en formato json en un volumen `logs`

# my-rest-api
Es un API con NestJs que incluye:
 - un servicio de healthcheck
 - un controlador para añadir y consultar clientes con parámetros básicos `id` y `name`
 - un controlador para añadir y consultar productos con parámetros básicos `id` y `name`
 - un SqlLite para persistir los datos, aunque no se están almacenando en volumen y desaparecen al limpiar el contenedor
 - un middleware que registra las peticiones recibidas en formato json en un volumen `logs`

# traffic-generator
Es un script en JS que lanza llamadas a los POST y GET de los controladores de los dos apis con una cadencia entre 1s y 2s.
El objetivo es recibir llamadas en los APIs para ir generando log.


<img width="793" height="457" alt="image" src="https://github.com/user-attachments/assets/0b5099a3-df8b-49c7-a0f6-cf09bd0ed8c2" />
