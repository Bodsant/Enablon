import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RbacAdminService, RoleItem, PermissionItem } from './rbac-admin.service';

@Component({
  selector: 'app-rbac-admin-page',
  standalone: true,
  imports: [CommonModule],
  providers: [RbacAdminService],
  template: `
    <section class="rbac-admin-page">
      <p class="eyebrow">IDENTITY &amp; ACCESS</p>
      <h1>RBAC administration</h1>
      <p>Roles and permissions governing role-based access control.</p>

      <h2>Roles <span class="count">{{ roles().length }}</span></h2>
      <ul class="list">
        @for (r of roles(); track r.id) {
          <li><strong>{{ r.code }}</strong> — {{ r.name }} <span class="badge">{{ r.scopeType }}</span>
            @if (r.isSystem) { <span class="badge">system</span> }</li>
        } @empty {
          <li class="muted">No roles loaded.</li>
        }
      </ul>

      <h2>Permissions <span class="count">{{ permissions().length }}</span></h2>
      <ul class="list">
        @for (p of permissions(); track p.id) {
          <li><strong>{{ p.code }}</strong> — {{ p.module }} <span class="badge">{{ p.action }}</span></li>
        } @empty {
          <li class="muted">No permissions loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class RbacAdminPageComponent implements OnInit {
  private readonly svc = inject(RbacAdminService);
  readonly roles = signal<RoleItem[]>([]);
  readonly permissions = signal<PermissionItem[]>([]);

  ngOnInit(): void {
    this.svc.listRoles().subscribe((x) => this.roles.set(x));
    this.svc.listPermissions().subscribe((x) => this.permissions.set(x));
  }
}