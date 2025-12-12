# 🚀 Tilevania Game Server - Deployment Guide

**Last Updated:** December 12, 2025

---

## 📋 Pre-Deployment Checklist

### Phase 5 Completion
- [x] Testing framework setup (Jest + Supertest)
- [x] Unit tests created and passing
- [x] Integration tests created (auth passing)
- [x] Swagger documentation live at /api/docs
- [ ] Code coverage ≥ 70% (currently ~35%)
- [ ] Performance optimization complete
- [ ] All integration tests passing

### Code Quality
- [ ] Linting passed (no warnings)
- [ ] Security audit passed
- [ ] Dependencies audit passed (npm audit)
- [ ] No console.log statements in production code

---

## 🌐 PHASE 6.1: Production Environment Setup

### Step 1: MongoDB Atlas Setup

**Prerequisites:**
- MongoDB Atlas account (free tier available)
- Credit card for account verification

**Instructions:**
1. Go to https://www.mongodb.com/cloud/atlas
2. Create a free account
3. Create a new project named "tilevania"
4. Create a cluster (free M0 tier is fine for MVP)
5. Create a database user with:
   - Username: `tilevania_admin`
   - Password: `[generate strong password]`
6. Whitelist IP: `0.0.0.0/0` (or your server IP for production)
7. Get connection string: `mongodb+srv://user:password@cluster.mongodb.net/tilevania?retryWrites=true&w=majority`

**Create .env.production:**
```bash
# Copy from .env and update:
cp .env .env.production

# Edit values:
NODE_ENV=production
PORT=3000
MONGODB_URI=mongodb+srv://tilevania_admin:PASSWORD@cluster.mongodb.net/tilevania?retryWrites=true&w=majority
JWT_SECRET=[generate new secret with: node -e "console.log(require('crypto').randomBytes(32).toString('hex'))"]
```

### Step 2: Generate Secure Secrets

```bash
# Generate JWT secret
node -e "console.log('JWT_SECRET=' + require('crypto').randomBytes(32).toString('hex'))"

# Generate API key for monitoring (optional)
node -e "console.log('API_KEY=' + require('crypto').randomBytes(16).toString('hex'))"
```

### Step 3: Environment Variables

**Required Variables:**
```
NODE_ENV=production
PORT=3000
MONGODB_URI=[production MongoDB Atlas URI]
JWT_SECRET=[secure random key]
LOG_LEVEL=info
```

**Optional Variables:**
```
SENTRY_DSN=[for error tracking]
REDIS_URL=[for caching]
CORS_ORIGIN=https://tilevania.com
```

---

## 🚀 PHASE 6.2: Deployment Options

### Option 1: Heroku (Easiest)

**Prerequisites:**
- Heroku account (free tier available)
- Heroku CLI installed

**Deployment Steps:**
```bash
# 1. Login to Heroku
heroku login

# 2. Create app
heroku create tilevania-gameserver

# 3. Set environment variables
heroku config:set NODE_ENV=production
heroku config:set MONGODB_URI=mongodb+srv://...
heroku config:set JWT_SECRET=...

# 4. Deploy
git push heroku main

# 5. View logs
heroku logs --tail
```

**Costs:** Free tier available (~$5/month with credits)

**Pros:**
- Very easy setup
- Built-in CI/CD
- Automatic restarts
- Good for MVP

**Cons:**
- Limited free tier (550 hours/month)
- Slower performance
- No SSH access on free tier

---

### Option 2: Railway (Recommended)

**Prerequisites:**
- Railway account (free tier available)
- GitHub repository

**Deployment Steps:**
1. Go to https://railway.app
2. Create account and connect GitHub
3. Create new project → Deploy from GitHub repo
4. Select repository and branch
5. Add environment variables in Railway dashboard
6. Automatic deployment on git push

**Costs:** Free tier with $5 monthly credit

**Pros:**
- Very modern
- Simple interface
- Automatic deployments
- Good documentation

**Cons:**
- Newer platform
- Less community support

---

### Option 3: Render

**Prerequisites:**
- Render account
- GitHub repository

**Deployment Steps:**
1. Go to https://render.com
2. Create account and connect GitHub
3. Create new "Web Service"
4. Select repository
5. Configure:
   - Build command: `npm install`
   - Start command: `npm start`
   - Environment: Node
6. Add environment variables
7. Click "Deploy"

**Costs:** Free tier available (deploys sleep after 15 min inactivity)

**Pros:**
- Free tier sufficient for learning
- Good performance
- Easy setup

**Cons:**
- Sleeps after inactivity (free tier)
- Limited resources

---

### Option 4: AWS EC2

**Prerequisites:**
- AWS account
- t2.micro instance (free tier eligible)

**Deployment Steps:**
```bash
# 1. SSH into EC2 instance
ssh -i key.pem ubuntu@instance-ip

# 2. Install Node.js
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs npm

# 3. Install MongoDB (or use Atlas)
# (skip if using MongoDB Atlas)

# 4. Clone repository
git clone <your-repo-url>
cd GameServer

# 5. Install dependencies
npm install

# 6. Create .env file
nano .env

# 7. Install PM2 for process management
sudo npm install -g pm2

# 8. Start application
pm2 start server.js --name "tilevania"
pm2 save
pm2 startup

# 9. Setup Nginx reverse proxy (optional)
# Use Let's Encrypt for SSL certificates
```

**Costs:** Free tier (t2.micro) for 12 months

**Pros:**
- Full control
- Best performance
- Scalable

**Cons:**
- More complex setup
- Need to manage updates
- More security responsibility

---

### Option 5: DigitalOcean

**Prerequisites:**
- DigitalOcean account
- $5/month droplet

**Deployment Steps:**
Similar to AWS EC2 but simpler.
1. Create droplet (Ubuntu 20.04, $5/month)
2. SSH into droplet
3. Follow AWS steps 2-9

**Costs:** $5/month (very affordable)

**Pros:**
- Simple interface
- Good documentation
- Affordable

**Cons:**
- Requires some system administration
- Must manage updates

---

## 🎯 Recommended Setup (MVP)

**For quick MVP deployment:**

```
Option: Railway or Render
Reason: Easiest, free tier sufficient, automatic deployments
Time to deploy: 10-15 minutes
Cost: Free

Fallback: Heroku if Railway doesn't work
Time to deploy: 15-20 minutes
Cost: Free with credits
```

---

## 🔒 PHASE 6.3: Monitoring & Logging

### Application Logging

**Setup Winston Logger:**
```bash
npm install winston
```

**[src/config/logger.js]:**
```javascript
const winston = require('winston');

const logger = winston.createLogger({
  level: 'info',
  format: winston.format.json(),
  transports: [
    new winston.transports.File({ filename: 'logs/error.log', level: 'error' }),
    new winston.transports.File({ filename: 'logs/combined.log' }),
  ],
});

if (process.env.NODE_ENV !== 'production') {
  logger.add(new winston.transports.Console({
    format: winston.format.simple(),
  }));
}

module.exports = logger;
```

### Error Tracking (Sentry)

**Setup Sentry:**
```bash
npm install @sentry/node
```

**[server.js]:**
```javascript
const Sentry = require('@sentry/node');

Sentry.init({ dsn: process.env.SENTRY_DSN });

app.use(Sentry.Handlers.requestHandler());
app.use(Sentry.Handlers.errorHandler());
```

**Setup Sentry:**
1. Go to https://sentry.io
2. Create account
3. Create project (Node.js)
4. Get DSN
5. Add to .env.production: `SENTRY_DSN=https://...`

### Database Monitoring

**MongoDB Atlas Built-in Monitoring:**
- Automatic performance monitoring
- Real-time alerts
- Query performance analysis

**Setup Alerts:**
1. MongoDB Atlas → Project → Alerts
2. Create alerts for:
   - CPU usage > 80%
   - Memory usage > 80%
   - Connection count > 1000
   - Query execution time > 1000ms

### Performance Monitoring (Optional)

**New Relic (Free Tier):**
```bash
npm install newrelic
```

---

## 🔄 PHASE 6.4: CI/CD Pipeline

### GitHub Actions Setup

**[.github/workflows/test-and-deploy.yml]:**

```yaml
name: Test and Deploy

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  test:
    runs-on: ubuntu-latest
    services:
      mongodb:
        image: mongo:5.0
        options: >-
          --health-cmd mongosh
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 27017:27017

    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
          cache: 'npm'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Run linter
        run: npm run lint
      
      - name: Run tests
        run: npm test
        env:
          MONGODB_URI: mongodb://localhost:27017/tilevania_test
      
      - name: Generate coverage report
        run: npm run test:coverage
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3

  deploy:
    needs: test
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Deploy to Railway
        uses: railway-app/deploy@v1
        with:
          railway-token: ${{ secrets.RAILWAY_TOKEN }}
```

### Secret Management

**Add GitHub Secrets:**
1. Go to Settings → Secrets and variables → Actions
2. Add:
   - `RAILWAY_TOKEN` - Railway deployment token
   - `MONGODB_URI` - Production MongoDB URI
   - `JWT_SECRET` - Production JWT secret

---

## 📊 Post-Deployment Steps

### 1. Verify Deployment

```bash
# Check application health
curl https://your-domain.com/api/health

# Should return:
{
  "status": "OK",
  "message": "Tilevania Server is running",
  "timestamp": "2025-12-12T..."
}
```

### 2. Setup Domain

**If using custom domain:**
1. Purchase domain (GoDaddy, Namecheap, etc.)
2. Update DNS to point to deployment platform
3. Wait for DNS propagation (5-48 hours)

### 3. Setup SSL Certificate

**Automatic (recommended):**
- Most platforms (Railway, Render, Heroku) provide automatic SSL
- Just set custom domain

**Manual (AWS/DigitalOcean):**
```bash
# Install Certbot
sudo apt-get install certbot python3-certbot-nginx

# Get certificate
sudo certbot certonly --nginx -d yourdomain.com
```

### 4. Test All Features

```bash
# Test registration
curl -X POST http://localhost:3000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test","email":"test@example.com","password":"pass123"}'

# Test login
curl -X POST http://localhost:3000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"pass123"}'

# Check Swagger docs
curl http://localhost:3000/api/docs
```

---

## 🔐 Security Checklist

- [ ] HTTPS enabled
- [ ] JWT secrets are strong and unique
- [ ] MongoDB password is strong
- [ ] Firewall rules configured
- [ ] Rate limiting enabled
- [ ] CORS properly configured
- [ ] API keys stored in environment variables
- [ ] No sensitive data in logs
- [ ] Regular backups enabled
- [ ] Security headers configured

---

## 📈 Scaling for Production

### When to Scale

**Indicators:**
- 100+ concurrent users
- API response time > 500ms
- Database CPU > 80%
- Memory usage > 1GB

### Scaling Strategies

1. **Vertical Scaling:** Upgrade instance size
2. **Horizontal Scaling:** Multiple instances + load balancer
3. **Database:** Upgrade MongoDB Atlas cluster tier
4. **Caching:** Add Redis for session/leaderboard caching
5. **CDN:** Use CloudFront for static assets

---

## 🆘 Troubleshooting

### Common Issues

**Issue: "Cannot connect to MongoDB"**
- Check MONGODB_URI in .env
- Verify IP whitelist in MongoDB Atlas
- Check database password

**Issue: "JWT verification failed"**
- Verify JWT_SECRET is same in all environments
- Check token hasn't expired

**Issue: "CORS errors"**
- Update CORS_ORIGIN in .env
- Check that client origin matches

**Issue: "502 Bad Gateway"**
- Check application logs
- Verify database connection
- Check port is correct

---

## 📞 Support Resources

- **Documentation:** [README.md](README.md)
- **API Docs:** http://localhost:3000/api/docs
- **Issues:** Check GitHub issues
- **Logs:** Check deployment platform logs

---

**Next Steps:**
1. Complete Phase 5 testing (increase coverage to 70%)
2. Choose deployment platform (Railway recommended)
3. Setup MongoDB Atlas
4. Create .env.production
5. Deploy and test
6. Monitor logs and performance

