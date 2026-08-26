<template>
  <div class="shell">
    <!-- SIDEBAR ICON (kiri tipis) -->
    <aside class="rail">
      <div class="rail-top">
        <div class="logo">KCI</div>
      </div>

      <nav class="rail-nav">
        <button
          class="rail-item"
          :class="{ active: isActive('/dashboard') }"
          @click="go('/dashboard')"
          title="Home"
        >
          <span class="ico">⌂</span>
          <span class="lbl">Home</span>
        </button>

        <button
          class="rail-item"
          :class="{ active: isActive('/assets') }"
          @click="go('/assets')"
          title="Assets"
        >
          <span class="ico">▦</span>
          <span class="lbl">Assets</span>
        </button>

        <!-- Reports: buka drawer -->
        <button
          class="rail-item"
          :class="{ active: drawerOpen }"
          @click="toggleDrawer()"
          title="Reports"
        >
          <span class="ico">▣</span>
          <span class="lbl">Reports</span>
        </button>

        <button
          class="rail-item"
          :class="{ active: isActive('/events') }"
          @click="go('/events')"
          title="Events"
        >
          <span class="ico">🗂</span>
          <span class="lbl">Events</span>
        </button>
      </nav>

      <div class="rail-bottom">
        <div class="account">
          <div class="avatar">CJ</div>
          <div class="acct-label">Account</div>
        </div>
      </div>
    </aside>

    <!-- DRAWER (panel putih kiri) -->
    <transition name="slide">
      <aside v-if="drawerOpen" class="drawer">
        <div class="drawer-head">
          <div class="drawer-title">Reports</div>
          <button class="x" @click="drawerOpen = false">✕</button>
        </div>

        <div class="drawer-list">
          <button class="drawer-link" @click="goFromDrawer('/events')">
            Hazard Identification Reports</button
          ><button class="drawer-link" @click="goFromDrawer('/events')">
            Safety Walk & Talk Reports
          </button>
          <button class="drawer-link" @click="goFromDrawer('/assets')">
            Asset Reports
          </button>
          <button class="drawer-link" @click="goFromDrawer('/dashboard')">
            Dashboards
          </button>
        </div>
      </aside>
    </transition>

    <!-- OVERLAY -->
    <div v-if="drawerOpen" class="overlay" @click="drawerOpen = false"></div>

    <!-- MAIN -->
    <div class="main">
      <!-- TOPBAR -->
      <header class="topbar">
        <div class="crumb">
          <span class="crumb-muted">{{ area }}</span>
          <span class="crumb-sep">›</span>
          <span class="crumb-strong">{{ page }}</span>
        </div>

        <div class="top-actions">
          <input class="search" placeholder="Search by name and description" />
          <button class="iconbtn">⋯</button>
          <button class="iconbtn">i</button>
        </div>
      </header>

      <main class="content">
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from "vue";
import { useRoute, useRouter } from "vue-router";

const router = useRouter();
const route = useRoute();

const drawerOpen = ref(false);

function toggleDrawer() {
  drawerOpen.value = !drawerOpen.value;
}

function go(path) {
  router.push(path);
}

function goFromDrawer(path) {
  drawerOpen.value = false;
  router.push(path);
}

function isActive(prefix) {
  return route.path.startsWith(prefix);
}

const area = computed(() => {
  if (route.path.startsWith("/events")) return "Reports";
  if (route.path.startsWith("/assets")) return "Assets";
  return "Home";
});

const page = computed(() => {
  if (route.path.startsWith("/events")) return "Event Reports";
  if (route.path.startsWith("/assets")) return "Assets";
  return "Home";
});
</script>

<style scoped>
.shell {
  height: 100vh;
  display: grid;
  grid-template-columns: 64px 1fr;
  overflow: hidden;
  font-family: Arial, sans-serif;
  background: #f3f4f6;
}

/* RAIL */
.rail {
  height: 100vh;
  background: #0f0f10;
  color: #e5e7eb;
  display: flex;
  flex-direction: column;
}

.rail-top {
  height: 56px;
  display: grid;
  place-items: center;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.logo {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  background: #fff;
  color: #111;
  display: grid;
  place-items: center;
  font-weight: 800;
  font-size: 12px;
}

.rail-nav {
  padding: 10px 6px;
  display: grid;
  gap: 10px;
}

.rail-item {
  border: none;
  background: transparent;
  color: rgba(229, 231, 235, 0.85);
  cursor: pointer;
  padding: 8px 6px;
  border-radius: 12px;
  display: grid;
  justify-items: center;
  gap: 6px;
}

.rail-item:hover {
  background: rgba(255, 255, 255, 0.06);
}

.rail-item.active {
  background: rgba(255, 255, 255, 0.1);
  outline: 2px solid rgba(59, 130, 246, 0.55);
  outline-offset: -2px;
}

.ico {
  font-size: 18px;
  line-height: 1;
}

.lbl {
  font-size: 11px;
  opacity: 0.9;
}

.rail-bottom {
  margin-top: auto;
  padding: 10px 6px;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
}

.account {
  display: grid;
  justify-items: center;
  gap: 6px;
}

.avatar {
  width: 34px;
  height: 34px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.12);
  display: grid;
  place-items: center;
  font-weight: 700;
}

.acct-label {
  font-size: 11px;
  color: rgba(229, 231, 235, 0.75);
}

/* DRAWER */
.drawer {
  position: fixed;
  left: 64px;
  top: 0;
  height: 100vh;
  width: 360px;
  background: #fff;
  border-right: 1px solid #e5e7eb;
  z-index: 20;
  display: flex;
  flex-direction: column;
}

.drawer-head {
  height: 56px;
  padding: 10px 14px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid #e5e7eb;
}

.drawer-title {
  font-weight: 700;
  color: #111827;
}

.x {
  border: none;
  background: transparent;
  cursor: pointer;
  font-size: 18px;
}

.drawer-list {
  padding: 12px;
  display: grid;
  gap: 10px;
}

.drawer-link {
  text-align: left;
  padding: 12px;
  border-radius: 12px;
  border: 1px solid #e5e7eb;
  background: #fff;
  cursor: pointer;
}

.drawer-link:hover {
  background: #f3f4f6;
}

/* overlay */
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.25);
  z-index: 10;
}

/* animasi */
.slide-enter-active,
.slide-leave-active {
  transition: transform 0.18s ease;
}
.slide-enter-from,
.slide-leave-to {
  transform: translateX(-10px);
}

/* MAIN */
.main {
  height: 100vh;
  display: grid;
  grid-template-rows: 56px 1fr;
  overflow: hidden;
}

.topbar {
  background: #ffffff;
  border-bottom: 1px solid #e5e7eb;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 14px;
  box-sizing: border-box;
}

.crumb {
  display: flex;
  gap: 10px;
  align-items: center;
  color: #111827;
}

.crumb-muted {
  color: #6b7280;
  font-weight: 600;
}

.crumb-sep {
  color: #9ca3af;
}

.crumb-strong {
  font-weight: 700;
  color: #111827;
}

.top-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

.search {
  width: 360px;
  padding: 9px 10px;
  border: 1px solid #d1d5db;
  border-radius: 12px;
  outline: none;
}

.iconbtn {
  width: 34px;
  height: 34px;
  border-radius: 10px;
  border: 1px solid #e5e7eb;
  background: #fff;
  cursor: pointer;
}

.content {
  overflow: auto;
  padding: 18px;
  box-sizing: border-box;
}
</style>
