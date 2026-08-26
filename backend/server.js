require("dotenv").config();
const express = require("express");
const cors = require("cors");
const pool = require("./db");

const app = express();

console.log("BOOT server.js:", __filename);
console.log("CWD:", process.cwd());

app.use(
  cors({
    origin: "http://localhost:5173",
  })
);

app.use(express.json());

app.get("/", (req, res) => {
  res.json({
    status: "ok",
    message: "API berjalan",
    endpoints: ["/health", "/db-check", "/assets", "/routes"],
  });
});

app.get("/health", (req, res) => {
  res.json({ status: "ok", message: "API berjalan" });
});

app.get("/db-check", async (req, res) => {
  try {
    const [rows] = await pool.query("SELECT 1 AS test");
    res.json({ status: "ok", db: rows[0] });
  } catch (err) {
    res.status(500).json({ status: "error", message: err.message });
  }
});

app.get("/routes", (req, res) => {
  const routes = [];
  const stack = app._router?.stack || [];
  for (const layer of stack) {
    if (layer.route?.path) {
      const methods = Object.keys(layer.route.methods || {}).map((m) =>
        m.toUpperCase()
      );
      routes.push({ path: layer.route.path, methods });
    }
  }
  res.json({ status: "ok", routes });
});

app.get("/assets", async (req, res) => {
  try {
    const [rows] = await pool.query(
      `SELECT asset_id, asset_code, asset_name, asset_type, location, status,
              installed_date, notes, created_at, updated_at
       FROM asset
       ORDER BY asset_id DESC`
    );
    res.json({ status: "ok", data: rows });
  } catch (err) {
    res.status(500).json({ status: "error", message: err.message });
  }
});

const port = Number(process.env.PORT) || 3002;
app.listen(port, "0.0.0.0", () => {
  console.log(`Server jalan di http://localhost:${port}`);
});
app.post("/assets", async (req, res) => {
  try {
    const {
      asset_code,
      asset_name,
      asset_type = "Other",
      location = null,
      status = "Active",
      installed_date = null,
      notes = null,
    } = req.body;

    if (!asset_code || !asset_name) {
      return res.status(400).json({
        status: "error",
        message: "asset_code dan asset_name wajib diisi",
      });
    }

    const [result] = await pool.query(
      `INSERT INTO asset
       (asset_code, asset_name, asset_type, location, status, installed_date, notes)
       VALUES (?, ?, ?, ?, ?, ?, ?)`,
      [
        asset_code,
        asset_name,
        asset_type,
        location,
        status,
        installed_date,
        notes,
      ]
    );

    res.status(201).json({
      status: "ok",
      message: "Asset berhasil ditambahkan",
      asset_id: result.insertId,
    });
  } catch (err) {
    res.status(500).json({ status: "error", message: err.message });
  }
});
