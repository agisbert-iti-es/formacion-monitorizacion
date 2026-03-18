import { Column, Entity, PrimaryColumn } from 'typeorm';

@Entity()
export class Customer {
  @PrimaryColumn()
  id: number;

  @Column()
  name: string;
}
