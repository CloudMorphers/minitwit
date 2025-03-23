# -*- mode: ruby -*-
# vi: set ft=ruby :

dbserver_work_dir = Dir.mktmpdir
webserver_work_dir = Dir.mktmpdir

Vagrant.configure("2") do |config|
  if ENV["DIGITAL_OCEAN_ACCESS_TOKEN"].to_s.strip.empty?
    raise "Error: The DIGITAL_OCEAN_ACCESS_TOKEN environment variable must be set."
  end
  if ENV["PRIVATE_SSH_KEY_FILE"].to_s.strip.empty?
    raise "Error: The PRIVATE_SSH_KEY_FILE environment variable must be set."
  end
  if ENV["SSH_KEY_NAME"].to_s.strip.empty?
    raise "Error: The SSH_KEY_NAME environment variable must be set."
  end
  if ENV["USER_PASSWORD"].to_s.strip.empty?
    raise "Error: The USER_PASSWORD environment variable must be set."
  end
  if ENV["MINITWIT_DB_USER"].to_s.strip.empty?
    raise "Error: The MINITWIT_DB_USER environment variable must be set."
  end
  if ENV["MINITWIT_DB_PASSWORD"].to_s.strip.empty?
    raise "Error: The MINITWIT_DB_PASSWORD environment variable must be set."
  end
  if ENV["MINITWIT_DB_NAME"].to_s.strip.empty?
    raise "Error: The MINITWIT_DB_NAME environment variable must be set."
  end

  config.nfs.functional = false
  config.ssh.private_key_path = ENV["PRIVATE_SSH_KEY_FILE"]
  config.vm.box = "digital_ocean"
  config.vm.box_url = "https://github.com/devopsgroup-io/vagrant-digitalocean/raw/master/box/digital_ocean.box"
  config.vm.synced_folder ".", "/vagrant", disabled: true
  config.vm.provision "shell", path: "droplet_init.sh", env: { "USER_PASSWORD" => ENV["USER_PASSWORD"] }

  config.vm.define "dbserver", primary: true do |dbserver|
    dbserver.vm.provider :digital_ocean do |provider, override|
      provider.ssh_key_name = ENV["SSH_KEY_NAME"]
      provider.token = ENV["DIGITAL_OCEAN_ACCESS_TOKEN"]
      provider.image = "ubuntu-24-04-x64"
      provider.region = "fra1"
      provider.size = "s-1vcpu-2gb"
      provider.private_networking = true
      override.vm.synced_folder dbserver_work_dir, dbserver_work_dir, type: "rsync"
    end
    dbserver.vm.hostname = "dbserver"
    dbserver.trigger.before :up do |trigger|
      trigger.ruby do |env, machine|
        puts "Copying PostgreSQL data from Docker volume to tarball..."
        %x{
          cp DbServer/docker-compose.yml #{dbserver_work_dir}/docker-compose.yml
          docker run --rm --volume dbserver_minitwit_pg_data:/var/lib/postgresql/data -v \
          #{dbserver_work_dir}:/backup busybox tar czf /backup/minitwit_pg_data.tar.gz /var/lib/postgresql/data
        }
      end
    end
    dbserver.trigger.after :up do |trigger|
      trigger.ruby do |env, machine|
        droplet = VagrantPlugins::DigitalOcean::Provider.droplet(machine)
        private_network = droplet["networks"]["v4"].find { |network| network["type"] == "private" }
        private_ip_address = private_network["ip_address"]
        File.write("#{webserver_work_dir}/dbserver_private_ip.txt", private_ip_address)
      end
    end
    dbserver.trigger.after :up do |trigger|
      trigger.ruby do |env, machine|
        FileUtils.remove_entry dbserver_work_dir
      end
    end
    dbserver.vm.provision "shell", inline: <<-SHELL
      set -euo pipefail
      ufw allow ssh
      ufw allow from 10.0.0.0/8 to any port 5432 proto tcp
      ufw --force enable
    SHELL
    dbserver.vm.provision "shell", inline: <<-SHELL
      set -euo pipefail
      cd /home/stud
      mkdir dbserver
      mv #{dbserver_work_dir}/docker-compose.yml dbserver/docker-compose.yml
      mv #{dbserver_work_dir}/minitwit_pg_data.tar.gz dbserver/minitwit_pg_data.tar.gz
      chown -R stud:stud dbserver
      su stud
      cd dbserver
      echo MINITWIT_DB_USER=#{ENV["MINITWIT_DB_USER"]} >> .env
      echo MINITWIT_DB_PASSWORD=#{ENV["MINITWIT_DB_PASSWORD"]} >> .env
      echo MINITWIT_DB_NAME=#{ENV["MINITWIT_DB_NAME"]} >> .env
      docker volume create dbserver_minitwit_pg_data
      docker run --rm --volume dbserver_minitwit_pg_data:/var/lib/postgresql/data -v \
      $(pwd):/backup busybox tar xf /backup/minitwit_pg_data.tar.gz
      docker-compose up -d
    SHELL
  end

  config.vm.define "webserver", primary: false do |webserver|
    webserver.vm.provider :digital_ocean do |provider, override|
      provider.ssh_key_name = ENV["SSH_KEY_NAME"]
      provider.token = ENV["DIGITAL_OCEAN_ACCESS_TOKEN"]
      provider.image = "ubuntu-24-04-x64"
      provider.region = "fra1"
      provider.size = "s-1vcpu-1gb"
      provider.private_networking = true
      override.vm.synced_folder webserver_work_dir, webserver_work_dir, type: "rsync"
    end
    webserver.vm.hostname = "webserver"
    webserver.trigger.before :up do |trigger|
      trigger.ruby do |env, machine|
        puts "Copying MiniTwit app data from Docker volume to tarball..."
        %x{
          cp WebServer/docker-compose.yml #{webserver_work_dir}/docker-compose.yml
          cp WebServer/nginx.conf #{webserver_work_dir}/nginx.conf
          docker run --rm --volume webserver_minitwit_app_data:/data -v \
          #{webserver_work_dir}:/backup busybox tar czf /backup/minitwit_app_data.tar.gz /data
        }
      end
    end
    webserver.trigger.before :up do |trigger|
      trigger.ruby do |env, machine|
        puts "Waiting for dbserver's private IP address..."
        while !File.file?("#{webserver_work_dir}/dbserver_private_ip.txt")
          sleep(1)
        end
      end
    end
    webserver.trigger.after :up do |trigger|
      trigger.ruby do |env, machine|
        FileUtils.remove_entry webserver_work_dir
      end
    end
    webserver.vm.provision "shell", inline: <<-SHELL
      set -euo pipefail
      DBSERVER_IP=$(cat #{webserver_work_dir}/dbserver_private_ip.txt)
      echo $DBSERVER_IP dbserver >> /etc/hosts
      ufw allow ssh
      ufw allow http
      ufw allow 8080/tcp # To be removed
      ufw --force enable
    SHELL
    webserver.vm.provision "shell", inline: <<-SHELL
      set -euo pipefail
      cd /home/stud
      mkdir webserver
      mv #{webserver_work_dir}/docker-compose.yml webserver/docker-compose.yml
      mv #{webserver_work_dir}/nginx.conf webserver/nginx.conf
      mv #{webserver_work_dir}/minitwit_app_data.tar.gz webserver/minitwit_app_data.tar.gz
      chown -R stud:stud webserver
      su stud
      cd webserver
      echo MINITWIT_DB_HOST=dbserver >> .env
      echo MINITWIT_DB_USER=#{ENV["MINITWIT_DB_USER"]} >> .env
      echo MINITWIT_DB_PASSWORD=#{ENV["MINITWIT_DB_PASSWORD"]} >> .env
      echo MINITWIT_DB_NAME=#{ENV["MINITWIT_DB_NAME"]} >> .env
      docker volume create webserver_minitwit_app_data
      docker run --rm --volume webserver_minitwit_app_data:/data -v \
      $(pwd):/backup busybox tar xf /backup/minitwit_app_data.tar.gz
      docker-compose up -d
    SHELL
  end
end
