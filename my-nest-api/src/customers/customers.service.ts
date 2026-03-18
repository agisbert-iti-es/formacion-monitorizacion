import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { Customer } from './customer.entity';
import { CreateCustomerDto } from './dto/create-customer.dto';

@Injectable()
export class CustomersService {
  constructor(
    @InjectRepository(Customer)
    private readonly repo: Repository<Customer>,
  ) {}

  create(createCustomerDto: CreateCustomerDto) {
    const entity = this.repo.create(createCustomerDto);
    return this.repo.save(entity);
  }

  findAll() {
    return this.repo.find();
  }
}
