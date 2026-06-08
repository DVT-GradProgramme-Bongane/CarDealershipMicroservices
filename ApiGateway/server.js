const express = require('express');
const { createProxyMiddleware } = require('http-proxy-middleware');

const app = express();

app.use((req, res, next) => {
    res.header('Access-Control-Allow-Origin', '*');
    res.header('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE,OPTIONS');
    res.header('Access-Control-Allow-Headers', 'Content-Type, Authorization, Content-Length, X-Requested-With');
    if (req.method === 'OPTIONS') {
        return res.sendStatus(200);
    }
    next();
});

const routes = {
    inventory: process.env.INVENTORY_SERVICE_URL,
    staff: process.env.STAFF_SERVICE_URL,
    client: process.env.CLIENT_SERVICE_URL,
    'new-car-sales': process.env.NEW_CAR_SALES_SERVICE_URL,
    'used-car-sales': process.env.USED_CAR_SALES_SERVICE_URL,
    financing: process.env.FINANCING_SERVICE_URL,
    'accessories-suppliers': process.env.ACCESSORIES_SUPPLIERS_SERVICE_URL,
    maintenance: process.env.MAINTENANCE_SERVICE_URL,
    notification: process.env.NOTIFICATION_SERVICE_URL,
};

app.get('/health', (_req, res) => res.json({ status: 'ok' }));

for (const [name, target] of Object.entries(routes)) {
    if (!target) {
        console.warn(`Skipping /api/${name}: no target URL configured`);
        continue;
    }
    app.use(
        `/api/${name}`,
        createProxyMiddleware({
            target,
            changeOrigin: false,
            pathRewrite: { [`^/api/${name}`]: '' },
        })
    );
}
app.use(
    '/',
    createProxyMiddleware({
        target: process.env.FRONTEND_URL || 'http://frontend:3000',
        changeOrigin: false,
    })
);

app.listen(3000, () => console.log('API Gateway on :3000'));