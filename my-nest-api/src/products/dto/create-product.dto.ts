import { ApiProperty } from '@nestjs/swagger';

export class CreateProductDto {
  @ApiProperty({ example: 1 })
  id: number;

  @ApiProperty({ example: 'Mi producto' })
  name: string;
}
