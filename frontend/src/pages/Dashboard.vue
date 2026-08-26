<template>
  <AppLayout>
    <h1>Dashboard</h1>
    <div class="grid">
      <div class="card">
        <div class="title">Ringkasan</div>
        <div class="text">
          “Today, EHS practitioners continue to rely on injury rates,
          absenteeism, and other lagging metrics despite the growing acceptance
          of the fact that these failure-focused measures are ineffective in
          driving continuous improvement efforts. Leading indicators, on the
          other hand, appear to offer a more useful gauge of EHS activity by
          providing early warning signs of potential failure and, thus, enabling
          organizations to identify and correct deficiencies before they mature
          into accidents and injuries.”
          <div class="note"> 
          — Campbell Institute, Transforming EHS
          Performance Measurement Through Leading Indicators
          </div>
        </div>
      </div>
      <div class="card">
        <div class="title">Tips</div>
        <div class="text">
          Nanti kita isi metrik: total asset, active, maintenance, dll.
        </div>
      </div>
    </div>
  </AppLayout>
</template>

<script setup>
import AppLayout from "../layouts/AppLayout.vue";
</script>

<style scoped>
.grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
  margin-top: 12px;
}
.card {
  background: white;
  padding: 16px;
  border-radius: 14px;
  border: 1px solid #e5e7eb;
}
.title {
  font-weight: bold;
  margin-bottom: 6px;
}
.text {
  color: #374151;
  text-align: justify;
}
</style>
