import { NodeSDK } from '@opentelemetry/sdk-node';
import { getNodeAutoInstrumentations } from '@opentelemetry/auto-instrumentations-node';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-grpc';
import { OTLPMetricExporter } from '@opentelemetry/exporter-metrics-otlp-grpc';
import { OTLPLogExporter } from '@opentelemetry/exporter-logs-otlp-grpc';
import { PeriodicExportingMetricReader } from '@opentelemetry/sdk-metrics';
import { SimpleLogRecordProcessor } from '@opentelemetry/sdk-logs';
import { Resource } from '@opentelemetry/resources';
import { ATTR_SERVICE_NAME } from '@opentelemetry/semantic-conventions';
import * as Pyroscope from '@pyroscope/nodejs';

// 1. Pyroscope (Profiling)
Pyroscope.init({
  serverAddress: process.env.PYROSCOPE_SERVER_ADDRESS || 'http://pyroscope:4040',
  appName: 'frontend',
});
Pyroscope.start();

const OTLP_URL = process.env.OTEL_EXPORTER_OTLP_ENDPOINT || 'http://otel-collector:4317';

// 2. OpenTelemetry SDK
const sdk = new NodeSDK({
  resource: new Resource({
    [ATTR_SERVICE_NAME]: 'frontend',
  }),
  traceExporter: new OTLPTraceExporter({ url: OTLP_URL }),
  metricReader: new PeriodicExportingMetricReader({
    exporter: new OTLPMetricExporter({ url: OTLP_URL }),
    exportIntervalMillis: 10000,
  }),
  logRecordProcessor: new SimpleLogRecordProcessor(
    new OTLPLogExporter({ url: OTLP_URL })
  ) as any,
  instrumentations: [
    getNodeAutoInstrumentations({
      '@opentelemetry/instrumentation-fs': { enabled: false },
    }),
  ],
});

sdk.start();
console.log('--- Telemetry & Profiling Initialized ---');