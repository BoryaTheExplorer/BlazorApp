terraform {
  required_providers {
    aws = {
      source = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region     = "eu-north-1"
}

variable "subnet_prefix" {
  description = "cidr block for the subnet"
  type = string

}

resource "aws_vpc" "prod-vpc" {
  cidr_block = "10.0.0.0/16"

  tags       = {
    Name     = "Production-VPC"
  }
}

resource "aws_internet_gateway" "main-gateway" {
  vpc_id = aws_vpc.prod-vpc.id
}

resource "aws_route_table" "prod-route-table" {
  vpc_id                   = aws_vpc.prod-vpc.id

  route {
    cidr_block             = "0.0.0.0/0"
    gateway_id             = aws_internet_gateway.main-gateway.id
  }

  route {
    ipv6_cidr_block        = "::/0"
    gateway_id = aws_internet_gateway.main-gateway.id
  }

  tags                     = {
    Name                   = "Production"
  }
}

resource "aws_subnet" "subnet-1" {
  vpc_id            = aws_vpc.prod-vpc.id
  cidr_block        = "10.0.1.0/24"
  #cidr_block = var.subnet_prefix
  availability_zone = "eu-north-1a"

  tags              = {
    Name            = "Production-Subnet"
  }
}

resource "aws_route_table_association" "route-table-association" {
  subnet_id      = aws_subnet.subnet-1.id
  route_table_id = aws_route_table.prod-route-table.id
}

resource "aws_security_group" "allow-web" {
  name        = "allow-web-traffic"
  description = "Allow Web inbound traffic"
  vpc_id      = aws_vpc.prod-vpc.id

  ingress {
    description = "HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = [ "0.0.0.0/0" ]
  }
  ingress {
    description = "HTTP"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = [ "0.0.0.0/0" ]
  }
  ingress {
    description = "SSH"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = [ "0.0.0.0/0" ]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = [ "0.0.0.0/0" ]
  }

  tags          = {
    Name        = "Allow-Web"
  }
}

resource "aws_network_interface" "network-interface" {
  subnet_id       = aws_subnet.subnet-1.id
  private_ips     = [ "10.0.1.50" ]
  security_groups = [ aws_security_group.allow-web.id ]
}

resource "aws_eip" "eip" {
  domain                    = "vpc"
  network_interface         = aws_network_interface.network-interface.id
  associate_with_private_ip = "10.0.1.50"
  depends_on                = [ aws_internet_gateway.main-gateway, aws_instance.web-server-instance ]
}

output "server_public_ip" {
  value = aws_eip.eip.public_ip
}

resource "aws_instance" "web-server-instance" {
  ami               = "ami-0c1ac8a41498c1a9c"
  instance_type     = "t3.micro"
  availability_zone = "eu-north-1a"

  key_name          = "main-key"

  network_interface {
    device_index         = 0
    network_interface_id = aws_network_interface.network-interface.id
  }

  user_data              = <<-EOF
              #!/bin/bash
              sudo apt update -y
              sudo apt install apache2 -y
              sudo systemctl start apache2
              sudo bash -c 'echo My very first web server > /var/www/html/index.html'
              EOF
  tags                   = {
    Name                 = "Web-Server"
  }
}

output "server_private_ip" {
  value = aws_instance.web-server-instance.private_ip
}