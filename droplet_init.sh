#!/bin/bash
set -euo pipefail

SSH_DIR=/home/stud/.ssh

# Create new user and add to sudo group
useradd -m -s /bin/bash stud
usermod -aG sudo stud
echo "stud:$USER_PASSWORD" | chpasswd

# Copy SSH keys to new user
mkdir -p $SSH_DIR
cp /root/.ssh/authorized_keys $SSH_DIR/authorized_keys
chown -R stud:stud $SSH_DIR
chmod 700 $SSH_DIR
chmod 600 $SSH_DIR/authorized_keys

# Disable root SSH login and password authentication
sed -i 's/^PermitRootLogin yes/PermitRootLogin no/' /etc/ssh/sshd_config
sed -i 's/^#PasswordAuthentication yes/PasswordAuthentication no/' /etc/ssh/sshd_config
systemctl restart ssh

while fuser /var/lib/apt/lists/lock /var/lib/dpkg/lock /var/lib/dpkg/lock-frontend >/dev/null 2>&1; do
  echo "Waiting for apt/dpkg locks to be released..."
  sleep 5
done

# Add Docker's official GPG key
sudo apt-get update -y
sudo apt-get install -y ca-certificates curl
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc

# Add the repository to Apt sources
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] \
  https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo "${UBUNTU_CODENAME:-$VERSION_CODENAME}") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update -y

# Install docker packages
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose docker-compose-plugin

# Add user to docker group
usermod -aG docker stud
