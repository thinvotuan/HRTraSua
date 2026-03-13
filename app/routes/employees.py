from flask import Blueprint, render_template, request, redirect, url_for, flash
from app import db
from app.models import Employee, Department, Position
from datetime import date

employees_bp = Blueprint('employees', __name__)


@employees_bp.route('/')
def index():
    q = request.args.get('q', '').strip()
    dept_id = request.args.get('dept_id', '')
    status = request.args.get('status', '')

    query = Employee.query

    if q:
        query = query.filter(
            (Employee.full_name.ilike(f'%{q}%')) |
            (Employee.employee_code.ilike(f'%{q}%')) |
            (Employee.phone.ilike(f'%{q}%')) |
            (Employee.email.ilike(f'%{q}%'))
        )
    if dept_id:
        query = query.filter_by(department_id=int(dept_id))
    if status:
        query = query.filter_by(status=status)

    employees = query.order_by(Employee.employee_code).all()
    departments = Department.query.order_by(Department.name).all()

    return render_template(
        'employees/index.html',
        employees=employees,
        departments=departments,
        q=q,
        dept_id=dept_id,
        status=status,
    )


@employees_bp.route('/them', methods=['GET', 'POST'])
def create():
    departments = Department.query.order_by(Department.name).all()
    positions = Position.query.order_by(Position.name).all()

    if request.method == 'POST':
        employee_code = request.form.get('employee_code', '').strip()
        full_name = request.form.get('full_name', '').strip()
        date_of_birth_str = request.form.get('date_of_birth', '').strip()
        gender = request.form.get('gender', '')
        phone = request.form.get('phone', '').strip()
        email = request.form.get('email', '').strip()
        address = request.form.get('address', '').strip()
        department_id = request.form.get('department_id', '')
        position_id = request.form.get('position_id', '')
        hire_date_str = request.form.get('hire_date', '').strip()
        status = request.form.get('status', 'active')

        errors = []
        if not employee_code:
            errors.append('Mã nhân viên không được để trống.')
        if not full_name:
            errors.append('Họ tên không được để trống.')
        if Employee.query.filter_by(employee_code=employee_code).first():
            errors.append(f'Mã nhân viên "{employee_code}" đã tồn tại.')

        if errors:
            for e in errors:
                flash(e, 'danger')
            return render_template('employees/form.html',
                                   departments=departments, positions=positions,
                                   form_data=request.form)

        dob = None
        if date_of_birth_str:
            try:
                dob = date.fromisoformat(date_of_birth_str)
            except ValueError:
                pass

        hd = date.today()
        if hire_date_str:
            try:
                hd = date.fromisoformat(hire_date_str)
            except ValueError:
                pass

        emp = Employee(
            employee_code=employee_code,
            full_name=full_name,
            date_of_birth=dob,
            gender=gender,
            phone=phone,
            email=email,
            address=address,
            department_id=int(department_id) if department_id else None,
            position_id=int(position_id) if position_id else None,
            hire_date=hd,
            status=status,
        )
        db.session.add(emp)
        db.session.commit()
        flash(f'Thêm nhân viên {full_name} thành công!', 'success')
        return redirect(url_for('employees.index'))

    return render_template('employees/form.html',
                           departments=departments, positions=positions,
                           form_data=None)


@employees_bp.route('/<int:emp_id>')
def detail(emp_id):
    emp = Employee.query.get_or_404(emp_id)
    return render_template('employees/detail.html', emp=emp)


@employees_bp.route('/<int:emp_id>/sua', methods=['GET', 'POST'])
def update(emp_id):
    emp = Employee.query.get_or_404(emp_id)
    departments = Department.query.order_by(Department.name).all()
    positions = Position.query.order_by(Position.name).all()

    if request.method == 'POST':
        emp.full_name = request.form.get('full_name', '').strip()
        dob_str = request.form.get('date_of_birth', '').strip()
        emp.gender = request.form.get('gender', '')
        emp.phone = request.form.get('phone', '').strip()
        emp.email = request.form.get('email', '').strip()
        emp.address = request.form.get('address', '').strip()
        dept_id = request.form.get('department_id', '')
        pos_id = request.form.get('position_id', '')
        hd_str = request.form.get('hire_date', '').strip()
        emp.status = request.form.get('status', 'active')

        if not emp.full_name:
            flash('Họ tên không được để trống.', 'danger')
            return render_template('employees/form.html',
                                   emp=emp, departments=departments,
                                   positions=positions, form_data=request.form)

        if dob_str:
            try:
                emp.date_of_birth = date.fromisoformat(dob_str)
            except ValueError:
                pass
        else:
            emp.date_of_birth = None

        if hd_str:
            try:
                emp.hire_date = date.fromisoformat(hd_str)
            except ValueError:
                pass

        emp.department_id = int(dept_id) if dept_id else None
        emp.position_id = int(pos_id) if pos_id else None

        db.session.commit()
        flash(f'Cập nhật nhân viên {emp.full_name} thành công!', 'success')
        return redirect(url_for('employees.detail', emp_id=emp.id))

    return render_template('employees/form.html',
                           emp=emp, departments=departments,
                           positions=positions, form_data=None)


@employees_bp.route('/<int:emp_id>/xoa', methods=['POST'])
def delete(emp_id):
    emp = Employee.query.get_or_404(emp_id)
    name = emp.full_name
    db.session.delete(emp)
    db.session.commit()
    flash(f'Đã xóa nhân viên {name}.', 'success')
    return redirect(url_for('employees.index'))


@employees_bp.route('/positions-by-dept')
def positions_by_dept():
    """AJAX endpoint to get positions by department."""
    dept_id = request.args.get('dept_id')
    if dept_id:
        positions = Position.query.filter_by(
            department_id=int(dept_id)
        ).order_by(Position.name).all()
        return {'positions': [{'id': p.id, 'name': p.name} for p in positions]}
    return {'positions': []}
