<template>
  <div class="wrap">
    <h1>Daftar Asset Data Center</h1>

    <!-- Form tambah asset -->
    <AssetForm @created="fetchAssets" />

    <div class="toolbar">
      <button @click="fetchAssets" :disabled="loading">
        {{ loading ? "Loading..." : "Refresh" }}
      </button>
      <span v-if="error" class="error">{{ error }}</span>
    </div>

    <table v-if="assets.length" class="table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Code</th>
          <th>Nama</th>
          <th>Tipe</th>
          <th>Lokasi</th>
          <th>Status</th>
          <th>Installed</th>
          <th>Aksi</th>
        </tr>
      </thead>

      <tbody>
        <tr v-for="a in assets" :key="a.asset_id">
          <!-- MODE EDIT (inline) -->
          <template v-if="editingId === a.asset_id">
            <td>{{ a.asset_id }}</td>

            <td>
              <input v-model.trim="editForm.asset_code" />
            </td>

            <td>
              <input v-model.trim="editForm.asset_name" />
            </td>

            <td>
              <select v-model="editForm.asset_type">
                <option>Server</option>
                <option>Storage</option>
                <option>Network</option>
                <option>Power</option>
                <option>Cooling</option>
                <option>Other</option>
              </select>
            </td>

            <td>
              <input v-model.trim="editForm.location" />
            </td>

            <td>
              <select v-model="editForm.status">
                <option>Active</option>
                <option>Maintenance</option>
                <option>Retired</option>
              </select>
            </td>

            <td>
              <input type="date" v-model="editForm.installed_date" />
            </td>

            <td class="actions">
              <button @click="saveEdit(a.asset_id)" :disabled="loading">
                Simpan
              </button>
              <button class="ghost" @click="cancelEdit" :disabled="loading">
                Batal
              </button>
            </td>
          </template>

          <!-- MODE VIEW -->
          <template v-else>
            <td>{{ a.asset_id }}</td>
            <td>{{ a.asset_code }}</td>
            <td>{{ a.asset_name }}</td>
            <td>{{ a.asset_type }}</td>
            <td>{{ a.location ?? "-" }}</td>
            <td>{{ a.status }}</td>
            <td>{{ a.installed_date ?? "-" }}</td>
            <td class="actions">
              <button @click="startEdit(a)">Edit</button>
              <button
                class="danger"
                @click="remove(a.asset_id)"
                :disabled="loading"
              >
                Hapus
              </button>
            </td>
          </template>
        </tr>
      </tbody>
    </table>

    <p v-else-if="loading">Mengambil data...</p>
    <p v-else-if="!error">Data asset masih kosong.</p>
  </div>
</template>

<script setup>
import { onMounted, ref } from "vue";
import { api } from "../api";
import AssetForm from "./AssetForm.vue";

const assets = ref([]);
const loading = ref(false);
const error = ref("");

const editingId = ref(null);

const editForm = ref({
  asset_code: "",
  asset_name: "",
  asset_type: "Other",
  location: "",
  status: "Active",
  installed_date: "",
  notes: "",
});

async function fetchAssets() {
  loading.value = true;
  error.value = "";
  try {
    const res = await api.get("/assets");
    assets.value = res.data?.data ?? [];
  } catch (err) {
    error.value =
      err?.response?.data?.message || err.message || "Gagal ambil data";
  } finally {
    loading.value = false;
  }
}

function startEdit(a) {
  editingId.value = a.asset_id;

  // Pastikan installed_date untuk input type=date itu formatnya YYYY-MM-DD
  const installed =
    typeof a.installed_date === "string" ? a.installed_date.slice(0, 10) : "";

  editForm.value = {
    asset_code: a.asset_code ?? "",
    asset_name: a.asset_name ?? "",
    asset_type: a.asset_type ?? "Other",
    location: a.location ?? "",
    status: a.status ?? "Active",
    installed_date: installed || "",
    notes: a.notes ?? "",
  };
}

function cancelEdit() {
  editingId.value = null;
}

async function saveEdit(id) {
  // Validasi minimal
  if (!editForm.value.asset_code || !editForm.value.asset_name) {
    error.value = "Asset Code dan Asset Name wajib diisi.";
    return;
  }

  loading.value = true;
  error.value = "";
  try {
    await api.put(`/assets/${id}`, {
      asset_code: editForm.value.asset_code,
      asset_name: editForm.value.asset_name,
      asset_type: editForm.value.asset_type,
      location: editForm.value.location || null,
      status: editForm.value.status,
      installed_date: editForm.value.installed_date || null,
      notes: editForm.value.notes || null,
    });

    editingId.value = null;
    await fetchAssets();
  } catch (err) {
    error.value =
      err?.response?.data?.message || err.message || "Gagal update data";
  } finally {
    loading.value = false;
  }
}

async function remove(id) {
  const ok = confirm("Yakin hapus asset ini?");
  if (!ok) return;

  loading.value = true;
  error.value = "";
  try {
    await api.delete(`/assets/${id}`);
    await fetchAssets();
  } catch (err) {
    error.value =
      err?.response?.data?.message || err.message || "Gagal hapus data";
  } finally {
    loading.value = false;
  }
}

onMounted(fetchAssets);
</script>

<style scoped>
.wrap {
  max-width: 1000px;
  margin: 24px auto;
  font-family: Arial, sans-serif;
}

.toolbar {
  display: flex;
  gap: 12px;
  align-items: center;
  margin-bottom: 12px;
}

.error {
  color: #b00020;
}

.table {
  width: 100%;
  border-collapse: collapse;
}

.table th,
.table td {
  border: 1px solid #ddd;
  padding: 8px;
}

.table th {
  text-align: left;
}

.actions {
  display: flex;
  gap: 8px;
  align-items: center;
}

input,
select {
  padding: 6px 8px;
  border: 1px solid #ccc;
  border-radius: 8px;
  width: 100%;
  box-sizing: border-box;
}

button {
  padding: 8px 12px;
  cursor: pointer;
}

button.danger {
  border: 1px solid #b00020;
  color: #b00020;
  background: transparent;
}

button.ghost {
  border: 1px solid #ccc;
  background: transparent;
}
</style>
