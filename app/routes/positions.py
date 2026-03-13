from flask import Blueprint, render_template, request, redirect, url_for, flash
from app import db
from app.models import Position, Department

positions_bp = Blueprint('positions', __name__)


@positions_bp.route('/')
def index():
    dept_id = request.args.get('dept_id', '')
    query = Position.query
    if dept_id:
        query = query.filter_by(department_id=int(dept_id))
    positions = query.order_by(Position.name).all()
    departments = Department.query.order_by(Department.name).all()
    return render_template('positions/index.html', positions=positions,
                           departments=departments, dept_id=dept_id)


@positions_bp.route('/them', methods=['GET', 'POST'])
def create():
    departments = Department.query.order_by(Department.name).all()

    if request.method == 'POST':
        name = request.form.get('name', '').strip()
        department_id = request.form.get('department_id', '')
        base_salary_str = request.form.get('base_salary', '0').strip()

        if not name:
            flash('Tên chức vụ không được để trống.', 'danger')
            return render_template('positions/form.html', departments=departments,
                                   form_data=request.form)
        if not department_id:
            flash('Vui lòng chọn phòng ban.', 'danger')
            return render_template('positions/form.html', departments=departments,
                                   form_data=request.form)

        try:
            base_salary = float(base_salary_str.replace(',', ''))
        except ValueError:
            base_salary = 0

        pos = Position(
            name=name,
            department_id=int(department_id),
            base_salary=base_salary,
        )
        db.session.add(pos)
        db.session.commit()
        flash(f'Thêm chức vụ {name} thành công!', 'success')
        return redirect(url_for('positions.index'))

    return render_template('positions/form.html', departments=departments, form_data=None)


@positions_bp.route('/<int:pos_id>/sua', methods=['GET', 'POST'])
def update(pos_id):
    pos = Position.query.get_or_404(pos_id)
    departments = Department.query.order_by(Department.name).all()

    if request.method == 'POST':
        name = request.form.get('name', '').strip()
        department_id = request.form.get('department_id', '')
        base_salary_str = request.form.get('base_salary', '0').strip()

        if not name:
            flash('Tên chức vụ không được để trống.', 'danger')
            return render_template('positions/form.html', pos=pos,
                                   departments=departments, form_data=request.form)

        try:
            base_salary = float(base_salary_str.replace(',', ''))
        except ValueError:
            base_salary = 0

        pos.name = name
        pos.department_id = int(department_id) if department_id else pos.department_id
        pos.base_salary = base_salary
        db.session.commit()
        flash(f'Cập nhật chức vụ thành công!', 'success')
        return redirect(url_for('positions.index'))

    return render_template('positions/form.html', pos=pos, departments=departments,
                           form_data=None)


@positions_bp.route('/<int:pos_id>/xoa', methods=['POST'])
def delete(pos_id):
    pos = Position.query.get_or_404(pos_id)
    if pos.employees:
        flash('Không thể xóa chức vụ đang có nhân viên.', 'danger')
        return redirect(url_for('positions.index'))
    name = pos.name
    db.session.delete(pos)
    db.session.commit()
    flash(f'Đã xóa chức vụ {name}.', 'success')
    return redirect(url_for('positions.index'))
