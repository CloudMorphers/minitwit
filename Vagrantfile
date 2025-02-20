# -*- mode: ruby -*-
# vi: set ft=ruby :

$ip_file = "db_ip.txt"

Vagrant.configure("2") do |config|
  config.vm.box = 'digital_ocean'
  config.vm.box_url = "https://github.com/devopsgroup-io/vagrant-digitalocean/raw/master/box/digital_ocean.box"
  config.ssh.private_key_path = '~/.ssh/id_rsa'
  config.vm.synced_folder ".", "/vagrant", type: "rsync"

  config.vm.define "dbserver", primary: true do |server|
    server.vm.provider :digital_ocean do |provider|
      provider.ssh_key_name = ENV["SSH_KEY_NAME"]
      provider.token = ENV["DIGITAL_OCEAN_TOKEN"]
      provider.image = 'ubuntu-22-04-x64'
      provider.region = 'fra1'
      provider.size = 's-1vcpu-1gb'
      provider.privatenetworking = true
    end

    server.vm.hostname = "dbserver"

    server.trigger.after :up do |trigger|
      trigger.info = "Writing dbserver's IP to file..."
      trigger.ruby do |env, machine|
        remote_ip = machine.instance_variable_get(:@communicator).instance_variable_get(:@connection_ssh_info)[:host]
        File.write($ip_file, remote_ip)
      end
    end

    server.vm.provision "shell", inline: <<-SHELL
      echo "Waiting for APT lock to be released..."
      retries=0
      max_retries=30
      sleep_time=6

      while sudo fuser /var/lib/apt/lists/lock >/dev/null 2>&1 || \
            sudo fuser /var/lib/dpkg/lock >/dev/null 2>&1 || \
            sudo fuser /var/lib/dpkg/lock-frontend >/dev/null 2>&1; do
          if [[ $retries -ge $max_retries ]]; then
              echo "⚠️  Timeout waiting for APT lock! Exiting..."
              exit 1
          fi
          echo "APT is locked, waiting..."
          sleep $sleep_time
          retries=$((retries + 1))
      done

      echo "Removing stale locks (if any)..."
      sudo rm -f /var/lib/apt/lists/lock /var/lib/dpkg/lock /var/lib/dpkg/lock-frontend
      sudo dpkg --configure -a

      echo "Updating and Installing MongoDB..."
      sudo apt-get update -y
      sudo apt-get install -y gnupg curl

      curl -fsSL https://www.mongodb.org/static/pgp/server-7.0.asc | sudo gpg --dearmor -o /usr/share/keyrings/mongodb-server-7.0.gpg
      echo "deb [ arch=amd64,arm64 signed-by=/usr/share/keyrings/mongodb-server-7.0.gpg ] https://repo.mongodb.org/apt/ubuntu jammy/mongodb-org/7.0 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-7.0.list

      sudo apt-get update -y
      sudo apt-get install -y mongodb-org

      sudo systemctl enable mongod
      sudo systemctl start mongod
      sleep 5
      sudo systemctl status mongod --no-pager
    SHELL
  end

  config.vm.define "webserver", primary: false do |server|
    server.vm.provider :digital_ocean do |provider|
      provider.ssh_key_name = ENV["SSH_KEY_NAME"]
      provider.token = ENV["DIGITAL_OCEAN_TOKEN"]
      provider.image = 'ubuntu-22-04-x64'
      provider.region = 'fra1'
      provider.size = 's-1vcpu-1gb'
      provider.privatenetworking = true
    end

    server.vm.hostname = "webserver"

    server.trigger.before :up do |trigger|
      trigger.info = "Waiting to create server until dbserver's IP is available."
      trigger.ruby do |env, machine|
        while !File.file?($ip_file) do
          sleep(1)
        end
        db_ip = File.read($ip_file).strip()
        puts "Now, I have it..."
        puts db_ip
      end
    end

    server.trigger.after :provision do |trigger|
      trigger.ruby do |env, machine|
        File.delete($ip_file) if File.exists? $ip_file
      end
    end

    server.vm.provision "shell", inline: <<-SHELL
      export DB_IP=`cat /vagrant/db_ip.txt`
      echo "DB Server IP: $DB_IP"

      echo "Ensuring APT is not locked..."

      retries=0
      max_retries=50
      sleep_time=6

      while sudo lsof /var/lib/dpkg/lock >/dev/null 2>&1 || \
            sudo lsof /var/lib/dpkg/lock-frontend >/dev/null 2>&1 || \
            sudo lsof /var/lib/apt/lists/lock >/dev/null 2>&1; do
          if [[ $retries -ge $max_retries ]]; then
              echo "⛔ APT lock timeout! Exiting..."
              exit 1
          fi
          echo "APT is locked, retrying in $sleep_time sec..."
          sleep $sleep_time
          retries=$((retries + 1))
      done

      echo "Removing stale locks..."
      sudo rm -f /var/lib/apt/lists/lock /var/lib/dpkg/lock /var/lib/dpkg/lock-frontend
      sudo dpkg --configure -a

      echo "Installing Python dependencies..."
      sudo apt-get update -y
      sudo apt-get install -y python3 python3-pip python3-venv

      echo "Installing .NET SDK..."
      wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
      sudo dpkg -i packages-microsoft-prod.deb
      rm packages-microsoft-prod.deb
      sudo apt-get update -y
      sudo apt-get install -y dotnet-sdk-7.0

      echo "Starting Python App..."
      cd /vagrant
      pip3 install Flask-PyMongo
      nohup python3 minitwit.py > out.log &

      echo "================================================================="
      echo "=                            DONE                               ="
      echo "================================================================="
      echo "Navigate in your browser to:"
      THIS_IP=`hostname -I | cut -d" " -f1`
      echo "http://${THIS_IP}:5000"
    SHELL
  end

  config.vm.provision "shell", privileged: false, inline: <<-SHELL
    sudo apt-get update -y
  SHELL
end
