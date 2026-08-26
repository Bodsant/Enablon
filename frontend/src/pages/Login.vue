<template>
  <div class="wrap">
    <div class="card">
      <h1>Login</h1>
      <p class="sub">Masuk ke Enablon Data Center</p>

      <label>Email</label>
      <input v-model.trim="email" placeholder="admin@dc.local" />

      <label>Password</label>
      <input v-model.trim="password" type="password" placeholder="••••••••" />

      <button @click="login" :disabled="loading">
        {{ loading ? "Masuk..." : "Login" }}
      </button>

      <p v-if="error" class="err">{{ error }}</p>

      <p class="hint">
        (sementara) login bebas: isi apa saja, nanti kita ganti JWT beneran
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";

const router = useRouter();
const email = ref("");
const password = ref("");
const error = ref("");
const loading = ref(false);

async function login() {
  error.value = "";
  loading.value = true;

  // simulasi login (nanti diganti ke backend JWT)
  if (!email.value || !password.value) {
    error.value = "Email dan password wajib diisi.";
    loading.value = false;
    return;
  }

  localStorage.setItem("token", "dummy-token");
  loading.value = false;
  router.push("/dashboard");
}
</script>

<style scoped>
.wrap {
  min-height: 100vh;
  display: grid;
  place-items: center;
  background: #0f172a;
  font-family: Arial, sans-serif;
}
.card {
  width: 360px;
  background: white;
  padding: 22px;
  border-radius: 14px;
  display: grid;
  gap: 10px;
}
.sub {
  margin-top: -6px;
  color: #6b7280;
}
label {
  font-size: 14px;
  color: #374151;
}
input {
  padding: 10px;
  border: 1px solid #d1d5db;
  border-radius: 10px;
}
button {
  padding: 10px;
  border-radius: 10px;
  border: none;
  background: #111827;
  color: white;
  cursor: pointer;
}
.err {
  color: #b00020;
}
.hint {
  font-size: 12px;
  color: #6b7280;
}
</style>
