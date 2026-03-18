import { ApiProperty } from '@nestjs/swagger';

export class CreateCustomerDto {
  @ApiProperty({ example: 1 })
  id: number;

  @ApiProperty({ example: 'María' })
  name: string;
}
