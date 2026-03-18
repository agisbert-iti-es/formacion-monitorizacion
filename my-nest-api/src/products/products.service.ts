import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { Product } from './product.entity';
import { CreateProductDto } from './dto/create-product.dto';

@Injectable()
export class ProductsService {
  constructor(
    @InjectRepository(Product)
    private readonly repo: Repository<Product>,
  ) {}

  create(createProductDto: CreateProductDto) {
    const entity = this.repo.create(createProductDto);
    return this.repo.save(entity);
  }

  findAll() {
    return this.repo.find();
  }
}
