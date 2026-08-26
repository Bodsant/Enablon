<template>
  <form class="form" @submit.prevent="submit">
    <h2>Tambah Asset</h2>

    <div class="row">
      <label>Asset Code *</label>
      <input v-model.trim="form.asset_code" placeholder="SRV-003" />
    </div>

    <div class="row">
      <label>Asset Name *</label>
      <input v-model.trim="form.asset_name" placeholder="Server Lenovo SR650" />
    </div>

    <div class="row">
      <label>Type</label>
      <select v-model="form.asset_type">
        <option>Server</option>
        <option>Storage</option>
        <option>Network</option>
        <option>Power</option>
        <option>Cooling</option>
        <option>Other</option>
      </select>
    </div>

    <div class="row">
      <label>Location</label>
      <input v-model.trim="form.location" placeholder="Rack A3" />
    </div>

    <div class="row">
      <label>Status</label>
      <select v-model="form.status">
        <option>Active</option>
        <option>Maintenance</option>
        <option>Retired</option>
      </select>
    </div>

    <div class="row">
      <label>Installed Date</label>
      <input type="date" v-model="form.installed_date" />
    </div>

    <div class="row">
      <label>Notes</label>
      <textarea
        v-model.trim="form.notes"
        rows="3"
        placeholder="Catatan..."
      ></textarea>
    </div>

    <div class="toolbar">
      <button type="submit" :disabled="loading">
        {{ loading ? "Menyimpan..." : "Simpan" }}
      </button>
      <span v-if="error" class="error">{{ error }}</span>
      <span v-if="success" class="success">{{ success }}</span>
    </div>
  </form>
</template>

<script setup>
import { onMounted, ref } from "vue";
import { api } from "../api";
import AssetForm from "./AssetForm.vue";

const assets = ref([]);
const emit = defineEmits(["created"]);

const loading = ref(false);
const error = ref("");
const success = ref("");

const form = ref({
  asset_code: "",
  asset_name: "",
  asset_type: "Other",
  location: "",
  status: "Active",
  installed_date: "",
  notes: "",
});

async function submit() {
  error.value = "";
  success.value = "";

  if (!form.value.asset_code || !form.value.asset_name) {
    error.value = "Asset Code dan Asset Name wajib diisi.";
    return;
  }

  loading.value = true;
  try {
    await api.post("/assets", {
      asset_code: form.value.asset_code,
      asset_name: form.value.asset_name,
      asset_type: form.value.asset_type,
      location: form.value.location || null,
      status: form.value.status,
      installed_date: form.value.installed_date || null,
      notes: form.value.notes || null,
    });

    success.value = "Asset berhasil ditambahkan.";

    // reset form
    form.value.asset_code = "";
    form.value.asset_name = "";
    form.value.asset_type = "Other";
    form.value.location = "";
    form.value.status = "Active";
    form.value.installed_date = "";
    form.value.notes = "";

    emit("created"); // kasih tahu parent buat refresh tabel
  } catch (err) {
    error.value =
      err?.response?.data?.message || err.message || "Gagal menyimpan";
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.form {
  border: 1px solid #ddd;
  padding: 16px;
  border-radius: 10px;
  margin-bottom: 16px;
}
.row {
  display: grid;
  gap: 6px;
  margin-bottom: 10px;
}
input,
select,
textarea {
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 8px;
}
.toolbar {
  display: flex;
  gap: 12px;
  align-items: center;
}
button {
  padding: 8px 12px;
  cursor: pointer;
}
.error {
  color: #b00020;
}
.success {
  color: #0a7a2f;
}
</style>
