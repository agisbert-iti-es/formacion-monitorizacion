import { Injectable, NestMiddleware } from '@nestjs/common';
import { NextFunction, Request, Response } from 'express';
import { promises as fs } from 'fs';
import * as path from 'path';

@Injectable()
export class RequestLoggerMiddleware implements NestMiddleware {
  async use(req: Request, res: Response, next: NextFunction) {
    const now = new Date();
    const utcDate = now.getUTCDate().toString().padStart(2, '0');
    const utcMonth = (now.getUTCMonth() + 1).toString().padStart(2, '0');
    const utcYear = now.getUTCFullYear();

    const logDir = path.resolve(process.cwd(), 'logs');
    const logFile = path.join(logDir, `${utcDate}-${utcMonth}-${utcYear}-my-nest-api.log`);

    await fs.mkdir(logDir, { recursive: true }).catch(() => undefined);

    const timestamp = now.toISOString();
    const startTime = process.hrtime.bigint();

    let responseBody: any;
    const originalSend = res.send.bind(res);

    // Capture response body
    res.send = (body?: any) => {
      responseBody = body;
      return originalSend(body);
    };

    res.on('finish', async () => {
      const durationMs = Number(process.hrtime.bigint() - startTime) / 1_000_000;

      const entry = {
        timestampUtc: timestamp,
        method: req.method,
        url: req.originalUrl || req.url,
        request_body: req.body,
        response_status: res.statusCode,
        response_body,
        elapsed_milliseconds: durationMs,
      };

      const line = JSON.stringify(entry) + '\n';
      try {
        await fs.appendFile(logFile, line);
      } catch (error) {
        // eslint-disable-next-line no-console
        console.error('Failed to write request log', error);
      }
    });

    next();
  }
}
