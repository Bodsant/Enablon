<template>
  <form class="form" @submit.prevent="submit">
    <h2>Laporan Safety</h2>

    <div class="row">
      <label>
        <select v-model="selected">
          <option disabled value="">Please select one</option>
          <option>Hazard Identification</option>
          <option>Safety Walk and Talk Report</option>
          <option>Permit Report</option>
          <option>Take Five Report</option>
        </select>
      </label>
      <p>Selected: {{ selected }}</p>
      <input v-model.trim="form.asset_code" placeholder="SRV-003" />
    </div>
<!-- modal sesuai pilihan menampilkan file form yang berbeda-->
</template>

<script setup>
import { onMounted, ref } from "vue";
import { api } from "../api";
import AssetForm from "./AssetForm.vue";

// buat state untuk menyimpan pilihan dropdown
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
