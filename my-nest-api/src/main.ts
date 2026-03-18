import { NestFactory } from '@nestjs/core';
import { DocumentBuilder, SwaggerModule } from '@nestjs/swagger';
import { AppModule } from './app.module';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  const config = new DocumentBuilder()
    .setTitle('My Nest API')
    .setDescription('API docs for products and customers')
    .setVersion('1.0')
    .build();

  const document = SwaggerModule.createDocument(app, config);
  SwaggerModule.setup('/', app, document);

  const port = process.env.PORT ?? 3000;
  await app.listen(port);
  console.log(`Documentación de la API disponible en http://localhost:${port}/`);
}
bootstrap();
