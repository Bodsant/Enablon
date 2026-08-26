require("dotenv").config();
const express = require("express");
const cors = require("cors");
const pool = require("./db");

const app = express();

/* Pastikan file yang benar dijalankan */
console.log("BOOT server.js:", __filename);
console.log("CWD:", process.cwd());

app.use(cors());
app.use(express.json());

/* Root endpoint */
app.get("/", (req, res) => {
  res.json({
    status: "ok",
    message: "API berjalan",
    endpoints: [
      "/health",
      "/db-check",
      "/assets (GET, POST)",
      "/assets/:id (GET, PUT, DELETE)",
      "/routes",
    ],
  });
});

/* Health check */
app.get("/health", (req, res) => {
  res.json({ status: "ok", message: "API berjalan" });
});

/* DB check */
app.get("/db-check", async (req, res) => {
  try {
    const [rows] = await pool.query("SELECT 1 AS test");
    res.json({ status: "ok", db: rows[0] });
  } catch (err) {
    res.status(500).json({ status: "error", message: err.message });
  }
});

/* Lihat route yang terdaftar (kompatibel lintas versi Express) */
app.get("/routes", (req, res) => {
  const routes = [];

  const collect = (stack) => {
    for (const layer of stack || []) {
      if (layer?.route?.path) {
        const methods = Object.keys(layer.route.methods || {}).map((m) =>
          m.toUpperCase()
        );
        routes.push({ path: layer.route.path, methods });
      } else if (layer?.name === "router" && layer?.handle?.stack) {
        // Nested router
        collect(layer.handle.stack);
      }
    }
  };

  const stackA = app._router?.stack; // Express 4
  const stackB = app.router?.stack; // beberapa build/versi
  collect(stackA);
  collect(stackB);

  res.json({
    status: "ok",
    debug: {
      has_app__router: Boolean(app._router),
      has_app_router: Boolean(app.router),
      stackA_len: Array.isArray(stackA) ? stackA.length : 0,
      stackB_len: Array.isArray(stackB) ? stackB.length : 0,
    },
    routes,
  });
});

/* GET /assets: ambil daftar asset */
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

/* POST /assets: tambah asset */
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
    if (err?.code === "ER_DUP_ENTRY") {
      return res.status(409).json({
        status: "error",
        message: "asset_code sudah ada (duplikat)",
      });
    }
    res.status(500).json({ status: "error", message: err.message });
  }
});

/* GET /assets/:id: detail asset */
app.get("/assets/:id", async (req, res) => {
  try {
    const id = Number(req.params.id);
    if (!Number.isFinite(id)) {
      return res
        .status(400)
        .json({ status: "error", message: "ID tidak valid" });
    }

    const [rows] = await pool.query(
      `SELECT asset_id, asset_code, asset_name, asset_type, location, status,
              installed_date, notes, created_at, updated_at
       FROM asset
       WHERE asset_id = ?`,
      [id]
    );

    if (rows.length === 0) {
      return res
        .status(404)
        .json({ status: "error", message: "Asset tidak ditemukan" });
    }

    res.json({ status: "ok", data: rows[0] });
  } catch (err) {
    res.status(500).json({ status: "error", message: err.message });
  }
});

/* PUT /assets/:id: update asset (partial update) */
app.put("/assets/:id", async (req, res) => {
  try {
    const id = Number(req.params.id);
    if (!Number.isFinite(id)) {
      return res
        .status(400)
        .json({ status: "error", message: "ID tidak valid" });
    }

    // ubah undefined -> null agar aman untuk mysql2
    const toNull = (v) => (typeof v === "undefined" ? null : v);

    const asset_code = toNull(req.body.asset_code);
    const asset_name = toNull(req.body.asset_name);
    const asset_type = toNull(req.body.asset_type);
    const location = toNull(req.body.location);
    const status = toNull(req.body.status);
    const installed_date = toNull(req.body.installed_date);
    const notes = toNull(req.body.notes);

    const [result] = await pool.query(
      `UPDATE asset
       SET asset_code = COALESCE(?, asset_code),
           asset_name = COALESCE(?, asset_name),
           asset_type = COALESCE(?, asset_type),
           location = COALESCE(?, location),
           status = COALESCE(?, status),
           installed_date = COALESCE(?, installed_date),
           notes = COALESCE(?, notes)
       WHERE asset_id = ?`,
      [
        asset_code,
        asset_name,
        asset_type,
        location,
        status,
        installed_date,
        notes,
        id,
      ]
    );

    if (result.affectedRows === 0) {
      return res
        .status(404)
        .json({ status: "error", message: "Asset tidak ditemukan" });
    }

    res.json({ status: "ok", message: "Asset berhasil diupdate" });
  } catch (err) {
    if (err?.code === "ER_DUP_ENTRY") {
      return res.status(409).json({
        status: "error",
        message: "asset_code sudah ada (duplikat)",
      });
    }
    res.status(500).json({ status: "error", message: err.message });
  }
});

/* DELETE /assets/:id: hapus asset */
app.delete("/assets/:id", async (req, res) => {
  try {
    const id = Number(req.params.id);
    if (!Number.isFinite(id)) {
      return res
        .status(400)
        .json({ status: "error", message: "ID tidak valid" });
    }

    const [result] = await pool.query(`DELETE FROM asset WHERE asset_id = ?`, [
      id,
    ]);

    if (result.affectedRows === 0) {
      return res
        .status(404)
        .json({ status: "error", message: "Asset tidak ditemukan" });
    }

    res.json({ status: "ok", message: "Asset berhasil dihapus" });
  } catch (err) {
    res.status(500).json({ status: "error", message: err.message });
  }
});

/* Listen (cukup satu kali) */
const port = Number(process.env.PORT) || 3002;
app.listen(port, "0.0.0.0", () => {
  console.log(`Server jalan di http://localhost:${port}`);
});
