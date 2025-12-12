FROM node:18-alpine

WORKDIR /app

COPY package*.json ./

# 4. Cài đặt các thư viện
RUN npm install --production

# 5. Copy toàn bộ code còn lại vào container
COPY . .

# 6. Mở cổng 3000 (hoặc cổng bạn dùng)
EXPOSE 3000

# 7. Lệnh chạy server khi container khởi động
CMD ["node", "server.js"]
# (LƯU Ý: Nếu file chạy chính của bạn là index.js hay main.js thì sửa lại dòng trên cho đúng)