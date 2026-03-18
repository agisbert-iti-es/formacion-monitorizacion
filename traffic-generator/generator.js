const axios = require('axios');
const { faker } = require('@faker-js/faker');

const NEST_API_URL = process.env.NEST_API_URL || 'http://my-nest-api:3000';
const DOTNET_API_URL = process.env.DOTNET_API_URL || 'http://my-dotnet-api:8080';

// Definir las APIs disponibles
const apis = [
  {
    name: 'NestJS',
    baseUrl: NEST_API_URL,
    endpoints: ['customers', 'products']
  },
  {
    name: 'DotNet',
    baseUrl: DOTNET_API_URL,
    endpoints: ['players']
  }
];

// Función de utilidad para pausar la ejecución
const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

// Función para obtener un número aleatorio en un rango
const getRandomRange = (min, max) => Math.floor(Math.random() * (max - min + 1) + min);

async function startTraffic() {
  while (true) {
    // Seleccionar una API aleatoria
    const selectedApi = apis[Math.floor(Math.random() * apis.length)];
    const selectedEndpoint = selectedApi.endpoints[Math.floor(Math.random() * selectedApi.endpoints.length)];
    
    const payload = {
      id: faker.number.int({ min: 1, max: 999999 }),
      name: selectedEndpoint === 'customers' ? faker.person.fullName() : 
            selectedEndpoint === 'products' ? faker.commerce.productName() : 
            faker.person.fullName() // Para players
    };

    const pause = getRandomRange(1000, 2000);

    try {
      // 1. Enviar el POST
      await axios.post(`${selectedApi.baseUrl}/${selectedEndpoint}`, payload);
      console.log(`[${selectedApi.name}] [POST] ${selectedEndpoint}: ${payload.name}`);

      // --- PAUSA ALEATORIA (1s - 2s) ---
      await sleep(pause);

      // 2. Enviar el GET
      const response = await axios.get(`${selectedApi.baseUrl}/${selectedEndpoint}`);
      console.log(`[${selectedApi.name}] listado [GET] ${selectedEndpoint} (Total: ${response.data.length})`);

    } catch (error) {
      console.error(`[${selectedApi.name}] Error en ${selectedEndpoint}: ${error.code || error.message}`);
      // Si hay error, esperamos un poco más antes de reintentar
      await sleep(2000);
    }

    // --- PAUSA ALEATORIA (1s - 2s) ---
    await sleep(pause);
  }
}

console.log("Generador de llamadas con datos fake iniciado...");
startTraffic();