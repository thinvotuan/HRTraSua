from flask import Blueprint, render_template, request, redirect, url_for, flash
from app import db
from app.models import Department

departments_bp = Blueprint('departments', __name__)


@departments_bp.route('/')
def index():
    departments = Department.query.order_by(Department.name).all()
    return render_template('departments/index.html', departments=departments)


@departments_bp.route('/them', methods=['GET', 'POST'])
def create():
    if request.method == 'POST':
        name = request.form.get('name', '').strip()
        description = request.form.get('description', '').strip()

        if not name:
            flash('Tên phòng ban không được để trống.', 'danger')
            return render_template('departments/form.html', form_data=request.form)

        if Department.query.filter_by(name=name).first():
            flash(f'Phòng ban "{name}" đã tồn tại.', 'danger')
            return render_template('departments/form.html', form_data=request.form)

        dept = Department(name=name, description=description)
        db.session.add(dept)
        db.session.commit()
        flash(f'Thêm phòng ban {name} thành công!', 'success')
        return redirect(url_for('departments.index'))

    return render_template('departments/form.html', form_data=None)


@departments_bp.route('/<int:dept_id>/sua', methods=['GET', 'POST'])
def update(dept_id):
    dept = Department.query.get_or_404(dept_id)

    if request.method == 'POST':
        name = request.form.get('name', '').strip()
        description = request.form.get('description', '').strip()

        if not name:
            flash('Tên phòng ban không được để trống.', 'danger')
            return render_template('departments/form.html', dept=dept,
                                   form_data=request.form)

        existing = Department.query.filter_by(name=name).first()
        if existing and existing.id != dept_id:
            flash(f'Phòng ban "{name}" đã tồn tại.', 'danger')
            return render_template('departments/form.html', dept=dept,
                                   form_data=request.form)

        dept.name = name
        dept.description = description
        db.session.commit()
        flash(f'Cập nhật phòng ban thành công!', 'success')
        return redirect(url_for('departments.index'))

    return render_template('departments/form.html', dept=dept, form_data=None)


@departments_bp.route('/<int:dept_id>/xoa', methods=['POST'])
def delete(dept_id):
    dept = Department.query.get_or_404(dept_id)
    if dept.employees:
        flash('Không thể xóa phòng ban đang có nhân viên.', 'danger')
        return redirect(url_for('departments.index'))
    name = dept.name
    db.session.delete(dept)
    db.session.commit()
    flash(f'Đã xóa phòng ban {name}.', 'success')
    return redirect(url_for('departments.index'))
